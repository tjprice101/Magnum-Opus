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

        public override void PostUpdate()
        {
            Player player = Player;
            Item heldItem = player.HeldItem;

            // Check if player is using a MagnumOpus melee weapon
            bool isSwinging = player.itemAnimation > 0 && IsMagnumMeleeWeapon(heldItem);

            if (isSwinging)
            {
                // Calculate the weapon's position and rotation during swing
                float swingProgress = 1f - (float)player.itemAnimation / player.itemAnimationMax;
                
                // Get weapon drawing info
                Vector2 weaponPos = player.MountedCenter;
                float rotation = player.itemRotation;
                
                // Offset based on swing direction
                Vector2 offset = (rotation + MathHelper.PiOver4 * player.direction).ToRotationVector2() * (heldItem.width * 0.5f * heldItem.scale);
                Vector2 smearPos = weaponPos + offset;

                // Determine glow color based on weapon theme
                Color glowColor = GetWeaponGlowColor(heldItem.type);

                // Add new smear frame
                smearFrames.Add(new SmearFrame
                {
                    Position = smearPos,
                    Rotation = rotation,
                    Scale = heldItem.scale,
                    Direction = player.direction,
                    Alpha = 1f,
                    GlowColor = glowColor
                });

                // Limit frames
                while (smearFrames.Count > MaxSmearFrames)
                {
                    smearFrames.RemoveAt(0);
                }
            }
            else if (wasSwinging && !isSwinging)
            {
                // Clear frames when swing ends (with slight delay for visual continuity)
            }

            // Fade out existing frames
            for (int i = smearFrames.Count - 1; i >= 0; i--)
            {
                var frame = smearFrames[i];
                frame.Alpha -= 0.15f;
                smearFrames[i] = frame;

                if (frame.Alpha <= 0)
                {
                    smearFrames.RemoveAt(i);
                }
            }

            wasSwinging = isSwinging;
            lastItemType = heldItem?.type ?? -1;
        }

        /// <summary>
        /// Check if an item is a MagnumOpus melee weapon that should have the smear effect
        /// </summary>
        private bool IsMagnumMeleeWeapon(Item item)
        {
            if (item == null || item.IsAir) return false;
            if (item.DamageType != DamageClass.Melee) return false;
            if (item.useStyle != ItemUseStyleID.Swing) return false;

            // Check if it's from our mod
            if (item.ModItem == null) return false;
            if (item.ModItem.Mod.Name != "MagnumOpus") return false;

            return true;
        }

        /// <summary>
        /// Get the appropriate glow color based on weapon type/theme
        /// </summary>
        private Color GetWeaponGlowColor(int itemType)
        {
            // Eroica weapons - crimson/gold
            if (itemType == ModContent.ItemType<SakurasBlossom>() ||
                itemType == ModContent.ItemType<CelestialValor>())
            {
                return new Color(255, 120, 60); // Warm golden-orange
            }
            
            // Moonlight weapons - purple/silver
            if (itemType == ModContent.ItemType<IncisorOfMoonlight>() ||
                itemType == ModContent.ItemType<EternalMoon>())
            {
                return new Color(180, 120, 255); // Ethereal purple
            }
            
            // Swan Lake weapons - black/white/pearlescent
            if (itemType == ModContent.ItemType<CalloftheBlackSwan>())
            {
                return new Color(220, 220, 230); // Pearlescent white
            }

            // Default glow - white/silver
            return new Color(200, 220, 255);
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
    /// </summary>
    public class MeleeSmearDrawLayer : PlayerDrawLayer
    {
        public override Position GetDefaultPosition() => new BeforeParent(Terraria.DataStructures.PlayerDrawLayers.HeldItem);

        public override bool GetDefaultVisibility(Terraria.DataStructures.PlayerDrawSet drawInfo)
        {
            Player player = drawInfo.drawPlayer;
            var smearPlayer = player.GetModPlayer<MeleeSmearPlayer>();
            return smearPlayer.HasActiveSmear();
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
                float scale = frame.Scale * (0.9f + progress * 0.1f);
                
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
            // Only apply to MagnumOpus melee weapons with swing style
            if (entity.ModItem == null) return false;
            if (entity.ModItem.Mod.Name != "MagnumOpus") return false;
            if (entity.DamageType != DamageClass.Melee) return false;
            if (entity.useStyle != ItemUseStyleID.Swing) return false;
            return true;
        }

        public override void UseStyle(Item item, Player player, Rectangle heldItemFrame)
        {
            // Add extra particles during swing for all MagnumOpus melee weapons
            if (player.itemAnimation > 0 && Main.rand.NextBool(3))
            {
                // Get swing position
                Vector2 swingPos = player.MountedCenter + (player.itemRotation + MathHelper.PiOver4 * player.direction).ToRotationVector2() * (item.width * item.scale);
                
                // Spawn themed particles based on weapon
                Color glowColor = GetWeaponThemeColor(item.type);
                
                // Small sparkle dust
                Dust sparkle = Dust.NewDustDirect(swingPos, 1, 1, DustID.FireworkFountain_Yellow, 
                    player.velocity.X * 0.2f, player.velocity.Y * 0.2f, 100, glowColor, 0.8f);
                sparkle.noGravity = true;
                sparkle.fadeIn = 0.5f;
            }
        }

        private Color GetWeaponThemeColor(int itemType)
        {
            // Eroica weapons
            if (itemType == ModContent.ItemType<SakurasBlossom>() ||
                itemType == ModContent.ItemType<CelestialValor>())
            {
                return new Color(255, 180, 100);
            }
            
            // Moonlight weapons
            if (itemType == ModContent.ItemType<IncisorOfMoonlight>() ||
                itemType == ModContent.ItemType<EternalMoon>())
            {
                return new Color(200, 150, 255);
            }
            
            // Swan Lake weapons
            if (itemType == ModContent.ItemType<CalloftheBlackSwan>())
            {
                return new Color(220, 220, 235); // Pearlescent white
            }

            return Color.White;
        }
    }
}
