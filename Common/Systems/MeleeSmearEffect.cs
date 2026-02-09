using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.GameContent;
using Terraria.DataStructures;
using MagnumOpus.Content.Eroica.ResonantWeapons;
using MagnumOpus.Content.MoonlightSonata.ResonantWeapons;
using MagnumOpus.Content.MoonlightSonata.Weapons;
using MagnumOpus.Content.SwanLake.ResonantWeapons;
using MagnumOpus.Content.LaCampanella.ResonantWeapons;
using MagnumOpus.Content.EnigmaVariations.ResonantWeapons;
using MagnumOpus.Common.Systems.Particles;

namespace MagnumOpus.Common.Systems
{
    /// <summary>
    /// Melee Swing Smear Effect System
    /// Creates a layered ghosting/smear glow effect when swinging MagnumOpus melee weapons.
    /// Draws multiple fading afterimages of the weapon during swing animation.
    /// </summary>
    public class MeleeSmearPlayer : ModPlayer
    {
        // Store recent swing positions and rotations for the smear effect
        private const int MaxSmearFrames = 8;
        private List<SmearFrame> smearFrames = new List<SmearFrame>();
        private int lastItemType = -1;
        private bool wasSwinging = false;

        public struct SmearFrame
        {
            public Vector2 Position;
            public float Rotation;
            public float Scale;
            public int Direction;
            public float Alpha;
            public Color GlowColor;
        }

        /// <summary>
        /// DISABLED: Frame tracking for melee smear effect has been disabled.
        /// The spectral weapon copy drawing system is no longer active.
        /// Particle/trail effects are handled by GlobalWeaponVFXOverhaul and CalamityStyleVFX.
        /// </summary>
        public override void PostUpdate()
        {
            // DISABLED: No longer tracking smear frames as the spectral copy drawing is disabled.
            // Leaving this empty to prevent unnecessary computation.
            // The particle/trail VFX are now fully handled by GlobalWeaponVFXOverhaul and CalamityStyleVFX.
        }

        /// <summary>
        /// Check if an item is a MagnumOpus melee weapon that should have the smear effect
        /// </summary>
        private bool IsMagnumMeleeWeapon(Item item)
        {
            if (item == null || item.IsAir) return false;
            if (item.DamageType != DamageClass.Melee) return false;
            if (item.useStyle != ItemUseStyleID.Swing) return false;
            
            // Skip weapons with custom graphics (they handle their own VFX)
            if (item.noUseGraphic) return false;

            // Check if it's from our mod
            if (item.ModItem == null) return false;
            if (item.ModItem.Mod.Name != "MagnumOpus") return false;

            return true;
        }

        /// <summary>
        /// Get the appropriate glow color based on weapon type/theme (DYNAMIC via rarity)
        /// </summary>
        private Color GetWeaponGlowColor(Item item)
        {
            if (item == null || item.IsAir) return new Color(200, 220, 255);
            
            // Detect theme via rarity type
            int rarityType = item.rare;
            
            // Eroica - crimson/gold gradient
            if (rarityType == ModContent.RarityType<EroicaRarity>())
                return new Color(255, 120, 60); // Warm golden-orange
            
            // Moonlight Sonata - purple/silver
            if (rarityType == ModContent.RarityType<MoonlightSonataRarity>())
                return new Color(180, 120, 255); // Ethereal purple
            
            // Swan Lake - pearlescent white
            if (rarityType == ModContent.RarityType<SwanRarity>())
                return new Color(220, 220, 230); // Pearlescent white
            
            // La Campanella - infernal orange
            if (rarityType == ModContent.RarityType<LaCampanellaRarity>())
                return new Color(255, 100, 0); // Infernal orange
            
            // Enigma Variations - arcane purple
            if (rarityType == ModContent.RarityType<EnigmaVariationsRarity>())
                return new Color(140, 60, 200); // Arcane purple
            
            // Fate - cosmic pink/crimson
            if (rarityType == ModContent.RarityType<FateRarity>())
                return new Color(200, 80, 120); // Dark pink cosmic

            // Default glow - white/silver
            return new Color(200, 220, 255);
        }

        /// <summary>
        /// Get weapon theme type for special effects (DYNAMIC via rarity)
        /// </summary>
        public WeaponTheme GetWeaponTheme(Item item)
        {
            if (item == null || item.IsAir) return WeaponTheme.Default;
            
            int rarityType = item.rare;
            
            // La Campanella - heavy smoke
            if (rarityType == ModContent.RarityType<LaCampanellaRarity>())
                return WeaponTheme.LaCampanella;
                
            // Enigma - void/glyph effects
            if (rarityType == ModContent.RarityType<EnigmaVariationsRarity>())
                return WeaponTheme.Enigma;
                
            // Swan Lake - feather effects
            if (rarityType == ModContent.RarityType<SwanRarity>())
                return WeaponTheme.SwanLake;
                
            // Eroica - sakura effects
            if (rarityType == ModContent.RarityType<EroicaRarity>())
                return WeaponTheme.Eroica;
                
            // Moonlight - lunar effects
            if (rarityType == ModContent.RarityType<MoonlightSonataRarity>())
                return WeaponTheme.MoonlightSonata;
            
            // Fate - cosmic glyphs and stars
            if (rarityType == ModContent.RarityType<FateRarity>())
                return WeaponTheme.Fate;
                
            return WeaponTheme.Default;
        }
        
        public enum WeaponTheme
        {
            Default,
            Fate,
            LaCampanella,
            Enigma,
            SwanLake,
            Eroica,
            MoonlightSonata
        }

        /// <summary>
        /// Get smear frames for external rendering (used by the draw layer)
        /// </summary>
        public List<SmearFrame> GetSmearFrames() => smearFrames;

        /// <summary>
        /// Check if we should draw smear effect
        /// </summary>
        public bool HasActiveSmear() => smearFrames.Count > 0;
    }

    /// <summary>
    /// Draw layer that renders the melee smear effect
    /// DISABLED: The spectral weapon copy drawing has been removed.
    /// Particle/trail effects are now handled by GlobalWeaponVFXOverhaul and CalamityStyleVFX.
    /// </summary>
    public class MeleeSmearDrawLayer : PlayerDrawLayer
    {
        public override Position GetDefaultPosition() => new BeforeParent(Terraria.DataStructures.PlayerDrawLayers.HeldItem);

        /// <summary>
        /// Get theme-specific stretch intensity for Exoblade-style lengthwise stretch.
        /// Different themes have subtle variations in how much the weapon stretches during swing.
        /// </summary>
        private float GetThemeStretchIntensity(MeleeSmearPlayer.WeaponTheme theme)
        {
            return theme switch
            {
                MeleeSmearPlayer.WeaponTheme.Fate => 0.35f,        // Cosmic, reality-bending - max stretch
                MeleeSmearPlayer.WeaponTheme.SwanLake => 0.30f,    // Graceful, elegant - high stretch
                MeleeSmearPlayer.WeaponTheme.Eroica => 0.28f,      // Heroic, powerful - strong stretch
                MeleeSmearPlayer.WeaponTheme.LaCampanella => 0.25f, // Infernal, heavy - moderate stretch
                MeleeSmearPlayer.WeaponTheme.MoonlightSonata => 0.27f, // Mystical, flowing - moderate-high stretch
                MeleeSmearPlayer.WeaponTheme.Enigma => 0.32f,      // Mysterious, warping - high stretch
                _ => 0.25f                                          // Default stretch intensity
            };
        }

        public override bool GetDefaultVisibility(Terraria.DataStructures.PlayerDrawSet drawInfo)
        {
            // DISABLED: No longer drawing spectral weapon copies behind melee swings.
            // The particle/trail VFX are now handled by GlobalWeaponVFXOverhaul and CalamityStyleVFX.
            return false;
        }

        protected override void Draw(ref Terraria.DataStructures.PlayerDrawSet drawInfo)
        {
            Player player = drawInfo.drawPlayer;
            var smearPlayer = player.GetModPlayer<MeleeSmearPlayer>();
            var smearFrames = smearPlayer.GetSmearFrames();
            
            if (smearFrames.Count == 0) return;

            Item heldItem = player.HeldItem;
            if (heldItem == null || heldItem.IsAir) return;

            // Get weapon texture
            Texture2D texture = TextureAssets.Item[heldItem.type].Value;
            if (texture == null) return;
            
            // Get weapon theme for special effects (DYNAMIC)
            var weaponTheme = smearPlayer.GetWeaponTheme(heldItem);

            // === EXOBLADE-STYLE LENGTHWISE STRETCH ===
            // Calculate swing progress for the stretch effect
            // Formula: stretchScale = 1 + sin(swingProgress * PI) * stretchIntensity
            // This makes the weapon stretch at the middle of the swing and snap back
            float swingProgress = player.itemAnimationMax > 0 
                ? 1f - (float)player.itemAnimation / player.itemAnimationMax 
                : 0f;
            
            // Get theme-specific stretch intensity (subtle variations per theme)
            float stretchIntensity = GetThemeStretchIntensity(weaponTheme);
            float lengthwiseStretch = 1f + (float)Math.Sin(swingProgress * MathHelper.Pi) * stretchIntensity;
            
            // Draw smear afterimages (oldest first, so newest is on top)
            // This creates a layered, fluid glow trail effect
            for (int i = 0; i < smearFrames.Count; i++)
            {
                var frame = smearFrames[i];
                float progress = (float)i / smearFrames.Count;
                
                // Calculate alpha based on frame age and position in sequence
                // Newer frames (higher index) are more visible
                float baseAlpha = frame.Alpha * (0.2f + progress * 0.6f);
                
                // Scale increases slightly for older frames to create expanding trail feel
                // Apply Exoblade-style lengthwise stretch - newest frames get full stretch
                float baseScale = frame.Scale * (0.9f + progress * 0.1f);
                float stretchFactor = 1f + (lengthwiseStretch - 1f) * progress; // Gradual stretch application
                float scale = baseScale * stretchFactor;
                
                // Position relative to player
                Vector2 drawPos = player.MountedCenter - Main.screenPosition;
                
                // Determine flip and origin based on direction
                SpriteEffects effects;
                Vector2 origin;
                float finalRotation;
                
                if (frame.Direction == 1)
                {
                    // Facing right - standard rendering
                    effects = SpriteEffects.None;
                    origin = new Vector2(0f, texture.Height);
                    finalRotation = frame.Rotation + MathHelper.PiOver4;
                }
                else
                {
                    // Facing left - flip horizontally and adjust rotation/origin
                    effects = SpriteEffects.FlipHorizontally;
                    origin = new Vector2(texture.Width, texture.Height);
                    // When facing left, mirror the rotation to swing in correct direction
                    finalRotation = frame.Rotation - MathHelper.PiOver4;
                }

                // === LAYER 1: Outer soft glow (largest, most transparent) ===
                // This creates the soft outer aura of the smear
                Color outerGlowColor = frame.GlowColor * baseAlpha * 0.25f;
                outerGlowColor.A = 0; // Zero alpha for additive-like blending effect
                
                Terraria.DataStructures.DrawData outerGlow = new Terraria.DataStructures.DrawData(
                    texture,
                    drawPos,
                    null,
                    outerGlowColor,
                    finalRotation,
                    origin,
                    scale * 1.35f,
                    effects,
                    0
                );
                drawInfo.DrawDataCache.Add(outerGlow);

                // === LAYER 2: Mid glow layer ===
                // Slightly smaller, more saturated color
                Color midGlowColor = frame.GlowColor * baseAlpha * 0.4f;
                midGlowColor.A = 0;
                
                Terraria.DataStructures.DrawData midGlow = new Terraria.DataStructures.DrawData(
                    texture,
                    drawPos,
                    null,
                    midGlowColor,
                    finalRotation,
                    origin,
                    scale * 1.2f,
                    effects,
                    0
                );
                drawInfo.DrawDataCache.Add(midGlow);

                // === LAYER 3: Core colored silhouette ===
                // The main colored shape of the sword - solid color tint
                Color coreColor = frame.GlowColor * baseAlpha * 0.7f;
                coreColor.A = (byte)(baseAlpha * 180); // Semi-transparent core
                
                Terraria.DataStructures.DrawData coreData = new Terraria.DataStructures.DrawData(
                    texture,
                    drawPos,
                    null,
                    coreColor,
                    finalRotation,
                    origin,
                    scale * 1.05f,
                    effects,
                    0
                );
                drawInfo.DrawDataCache.Add(coreData);

                // === LAYER 4: Bright inner highlight (newest frames only) ===
                // Add extra brightness to the most recent frames
                if (progress > 0.5f)
                {
                    float highlightIntensity = (progress - 0.5f) * 2f; // 0 to 1 for last half
                    Color highlightColor = Color.Lerp(frame.GlowColor, Color.White, 0.5f) * baseAlpha * highlightIntensity * 0.6f;
                    highlightColor.A = 0;
                    
                    Terraria.DataStructures.DrawData highlight = new Terraria.DataStructures.DrawData(
                        texture,
                        drawPos,
                        null,
                        highlightColor,
                        finalRotation,
                        origin,
                        scale,
                        effects,
                        0
                    );
                    drawInfo.DrawDataCache.Add(highlight);
                }
                
                // === THEME-SPECIFIC EFFECTS ===
                
                // FATE: Chromatic Aberration - RGB channel separation for reality-bending
                if (weaponTheme == MeleeSmearPlayer.WeaponTheme.Fate && progress > 0.3f)
                {
                    float aberrationStrength = baseAlpha * 0.35f;
                    
                    // Red channel - offset left
                    Color redChannel = new Color(255, 0, 0, 0) * aberrationStrength;
                    Terraria.DataStructures.DrawData redAberration = new Terraria.DataStructures.DrawData(
                        texture, drawPos + new Vector2(-3, -1), null, redChannel,
                        finalRotation, origin, scale * 1.02f, effects, 0);
                    drawInfo.DrawDataCache.Add(redAberration);
                    
                    // Blue channel - offset right
                    Color blueChannel = new Color(0, 0, 255, 0) * aberrationStrength;
                    Terraria.DataStructures.DrawData blueAberration = new Terraria.DataStructures.DrawData(
                        texture, drawPos + new Vector2(3, 1), null, blueChannel,
                        finalRotation, origin, scale * 1.02f, effects, 0);
                    drawInfo.DrawDataCache.Add(blueAberration);
                    
                    // Spawn temporal echo particles
                    if (Main.rand.NextBool(4) && i == smearFrames.Count - 1)
                    {
                        Vector2 echoPos = player.MountedCenter + (frame.Rotation + MathHelper.PiOver4 * frame.Direction).ToRotationVector2() * 50f;
                        CustomParticles.GenericFlare(echoPos, new Color(180, 50, 100) * 0.6f, 0.35f, 12);
                    }
                }
                
                // LA CAMPANELLA: Heavy smoke trail for infernal atmosphere
                if (weaponTheme == MeleeSmearPlayer.WeaponTheme.LaCampanella && Main.rand.NextBool(3) && i == smearFrames.Count - 1)
                {
                    Vector2 smokePos = player.MountedCenter + (frame.Rotation + MathHelper.PiOver4 * frame.Direction).ToRotationVector2() * 45f;
                    Vector2 smokeVel = (frame.Rotation + MathHelper.PiOver2 * frame.Direction).ToRotationVector2() * 2f;
                    var smoke = new HeavySmokeParticle(smokePos, smokeVel, Color.Black, Main.rand.Next(20, 35), 0.4f, 0.7f, 0.015f, false);
                    MagnumParticleHandler.SpawnParticle(smoke);
                    
                    // Orange glow particles
                    if (Main.rand.NextBool(2))
                    {
                        CustomParticles.GenericFlare(smokePos, new Color(255, 100, 0) * 0.7f, 0.4f, 14);
                    }
                }
                
                // ENIGMA: Void/glyph effects for arcane mystery
                if (weaponTheme == MeleeSmearPlayer.WeaponTheme.Enigma && Main.rand.NextBool(5) && i == smearFrames.Count - 1)
                {
                    Vector2 glyphPos = player.MountedCenter + (frame.Rotation + MathHelper.PiOver4 * frame.Direction).ToRotationVector2() * 40f;
                    CustomParticles.Glyph(glyphPos, new Color(140, 60, 200) * 0.6f, 0.3f, -1);
                    
                    // Green flame accents
                    if (Main.rand.NextBool(2))
                    {
                        CustomParticles.GenericFlare(glyphPos + Main.rand.NextVector2Circular(10f, 10f), 
                            new Color(50, 220, 100) * 0.5f, 0.3f, 12);
                    }
                }
                
                // SWAN LAKE: Feather drift for graceful elegance
                if (weaponTheme == MeleeSmearPlayer.WeaponTheme.SwanLake && Main.rand.NextBool(4) && i == smearFrames.Count - 1)
                {
                    Vector2 featherPos = player.MountedCenter + (frame.Rotation + MathHelper.PiOver4 * frame.Direction).ToRotationVector2() * 50f;
                    ThemedParticles.SwanFeatherDrift(featherPos, Main.rand.NextBool() ? Color.White : new Color(20, 20, 30), 0.35f);
                }
                
                // EROICA: Sakura petals for heroic theme
                if (weaponTheme == MeleeSmearPlayer.WeaponTheme.Eroica && Main.rand.NextBool(4) && i == smearFrames.Count - 1)
                {
                    Vector2 petalPos = player.MountedCenter + (frame.Rotation + MathHelper.PiOver4 * frame.Direction).ToRotationVector2() * 45f;
                    ThemedParticles.SakuraPetals(petalPos, 1, 20f);
                }
                
                // MOONLIGHT SONATA: Prismatic lunar sparkles
                if (weaponTheme == MeleeSmearPlayer.WeaponTheme.MoonlightSonata && Main.rand.NextBool(3) && i == smearFrames.Count - 1)
                {
                    Vector2 sparklePos = player.MountedCenter + (frame.Rotation + MathHelper.PiOver4 * frame.Direction).ToRotationVector2() * 50f;
                    CustomParticles.GenericFlare(sparklePos, new Color(180, 120, 255) * 0.6f, 0.35f, 14);
                    
                    // Silver halo accent
                    if (Main.rand.NextBool(3))
                    {
                        CustomParticles.HaloRing(sparklePos, new Color(220, 220, 240) * 0.4f, 0.25f, 10);
                    }
                }
            }
        }
    }

    /// <summary>
    /// Alternative: Global Item approach for adding glow to melee weapons during use
    /// This adds the smear effect via PreDrawInInventory/UseItemFrame hooks
    /// </summary>
    public class MeleeSmearGlobalItem : GlobalItem
    {
        public override bool AppliesToEntity(Item entity, bool lateInstantiation)
        {
            // MASTER TOGGLE: When disabled, this global system does nothing
            if (!VFX.VFXMasterToggle.GlobalSystemsEnabled)
                return false;
            
            // Exclude debug weapons
            if (VFX.VFXExclusionHelper.ShouldExcludeItem(entity)) return false;
            
            // Only apply to MagnumOpus melee weapons with swing style
            if (entity.ModItem == null) return false;
            if (entity.ModItem.Mod.Name != "MagnumOpus") return false;
            if (entity.DamageType != DamageClass.Melee) return false;
            if (entity.useStyle != ItemUseStyleID.Swing) return false;
            // Skip weapons with custom graphics (they handle their own VFX)
            if (entity.noUseGraphic) return false;
            return true;
        }

        public override void UseStyle(Item item, Player player, Rectangle heldItemFrame)
        {
            // Add extra particles during swing for all MagnumOpus melee weapons
            if (player.itemAnimation > 0 && Main.rand.NextBool(3))
            {
                // Get swing position
                Vector2 swingPos = player.MountedCenter + (player.itemRotation + MathHelper.PiOver4 * player.direction).ToRotationVector2() * (item.width * item.scale);
                
                // Spawn themed particles based on weapon (DYNAMIC)
                Color glowColor = GetWeaponThemeColor(item);
                
                // Small sparkle dust
                Dust sparkle = Dust.NewDustDirect(swingPos, 1, 1, DustID.FireworkFountain_Yellow, 
                    player.velocity.X * 0.2f, player.velocity.Y * 0.2f, 100, glowColor, 0.8f);
                sparkle.noGravity = true;
                sparkle.fadeIn = 0.5f;
            }
        }

        private Color GetWeaponThemeColor(Item item)
        {
            if (item == null || item.IsAir) return Color.White;
            
            int rarityType = item.rare;
            
            // Eroica - sakura/gold
            if (rarityType == ModContent.RarityType<EroicaRarity>())
                return new Color(255, 180, 100);
            
            // Moonlight - purple/silver
            if (rarityType == ModContent.RarityType<MoonlightSonataRarity>())
                return new Color(200, 150, 255);
            
            // Swan Lake - pearlescent white
            if (rarityType == ModContent.RarityType<SwanRarity>())
                return new Color(220, 220, 235);
            
            // La Campanella - infernal orange
            if (rarityType == ModContent.RarityType<LaCampanellaRarity>())
                return new Color(255, 100, 0);
            
            // Enigma - arcane purple
            if (rarityType == ModContent.RarityType<EnigmaVariationsRarity>())
                return new Color(140, 60, 200);
            
            // Fate - cosmic pink
            if (rarityType == ModContent.RarityType<FateRarity>())
                return new Color(200, 80, 120);

            return Color.White;
        }
    }
}
