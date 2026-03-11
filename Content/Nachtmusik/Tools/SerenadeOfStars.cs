using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Audio;
using Terraria.DataStructures;
using ReLogic.Content;
using MagnumOpus.Content.Nachtmusik.ResonanceEnergies;
using MagnumOpus.Content.Nachtmusik.HarmonicCores;
using MagnumOpus.Content.Fate.CraftingStations;
using MagnumOpus.Common.Systems;
using MagnumOpus.Common.Systems.Particles;
using MagnumOpus.Common;
using MagnumOpus.Content.Nachtmusik;

namespace MagnumOpus.Content.Nachtmusik.Tools
{
    /// <summary>
    /// Serenade of Stars - Nachtmusik wings, the POST-FATE ULTIMATE tier wings.
    /// Features ethereal starlight trails, constellation patterns, and superior flight.
    /// Superior to Fate's Symphony of the Universe wings.
    /// Single-frame wings (no animation).
    /// </summary>
    [AutoloadEquip(EquipType.Wings)]
    public class SerenadeOfStars : ModItem
    {
        // Nachtmusik colors
        private static readonly Color DeepPurple = new Color(45, 27, 78);
        private static readonly Color Gold = new Color(255, 215, 0);
        private static readonly Color Violet = new Color(123, 104, 238);
        private static readonly Color StarWhite = new Color(255, 255, 255);

        public override void SetStaticDefaults()
        {
            ArmorIDs.Wing.Sets.Stats[Item.wingSlot] = new WingStats(
                flyTime: 480, // POST-FATE ULTIMATE (higher than Fate's 400)
                flySpeedOverride: 24f, // POST-FATE ULTIMATE (higher than Fate's 20)
                accelerationMultiplier: 5.0f, // POST-FATE ULTIMATE (higher than Fate's 4.2)
                hasHoldDownHoverFeatures: true
            );
        }

        public override void SetDefaults()
        {
            Item.width = 34;
            Item.height = 32;
            Item.accessory = true;
            Item.value = Item.buyPrice(platinum: 12);
            Item.rare = ModContent.RarityType<NachtmusikRarity>();
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "FlightTime", "8 second flight time") { OverrideColor = Gold });
            tooltips.Add(new TooltipLine(Mod, "FlightSpeed", "24 mph flight speed") { OverrideColor = Violet });
            tooltips.Add(new TooltipLine(Mod, "Hover", "Allows hovering") { OverrideColor = StarWhite });
            tooltips.Add(new TooltipLine(Mod, "DodgeEffect", "Double-tap to perform a celestial dash") { OverrideColor = DeepPurple });
            tooltips.Add(new TooltipLine(Mod, "Lore", "'Wings woven from the eternal night sky, carrying its radiant melody'") { OverrideColor = Color.Lerp(DeepPurple, Gold, 0.5f) });
        }

        public override void VerticalWingSpeeds(Player player, ref float ascentWhenFalling, ref float ascentWhenRising,
            ref float maxCanAscendMultiplier, ref float maxAscentMultiplier, ref float constantAscend)
        {
            ascentWhenFalling = 1.6f; // Higher than Fate
            ascentWhenRising = 0.32f;
            maxCanAscendMultiplier = 1.8f;
            maxAscentMultiplier = 5.2f;
            constantAscend = 0.28f;
        }
        
        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            player.GetModPlayer<SerenadeOfStarsPlayer>().hasWingsEquipped = true;
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ModContent.ItemType<NachtmusikResonantEnergy>(), 25)
                .AddIngredient(ModContent.ItemType<NachtmusikResonantCore>(), 12)
                .AddIngredient(ModContent.ItemType<HarmonicCoreOfNachtmusik>(), 2)
                .AddIngredient(ItemID.SoulofFlight, 60)
                .AddIngredient(ItemID.LunarBar, 20)
                .AddTile(ModContent.TileType<FatesCosmicAnvilTile>())
                .Register();
        }
    }

    public class SerenadeOfStarsPlayer : ModPlayer
    {
        // Nachtmusik colors
        private static readonly Color DeepPurple = new Color(45, 27, 78);
        private static readonly Color Gold = new Color(255, 215, 0);
        private static readonly Color Violet = new Color(123, 104, 238);
        private static readonly Color StarWhite = new Color(255, 255, 255);

        public bool hasWingsEquipped = false;
        private int dashCooldown = 0;
        private int doubleTapTimer = 0;
        private int lastDirection = 0;

        public override void ResetEffects()
        {
            hasWingsEquipped = false;
        }

        public override void PreUpdate()
        {
            if (dashCooldown > 0)
                dashCooldown--;

            // Double-tap dash detection
            if (hasWingsEquipped && dashCooldown <= 0)
            {
                int direction = 0;
                if (Player.controlLeft && !Player.controlRight)
                    direction = -1;
                else if (Player.controlRight && !Player.controlLeft)
                    direction = 1;

                if (direction != 0)
                {
                    if (doubleTapTimer > 0 && direction == lastDirection)
                    {
                        // Perform celestial dash
                        PerformCelestialDash(direction);
                        doubleTapTimer = 0;
                        dashCooldown = 45; // Cooldown
                    }
                    else if (direction != lastDirection || doubleTapTimer <= 0)
                    {
                        doubleTapTimer = 15; // Window for double-tap
                        lastDirection = direction;
                    }
                }

                if (doubleTapTimer > 0)
                    doubleTapTimer--;
            }
        }

        private void PerformCelestialDash(int direction)
        {
            // Dash velocity
            Player.velocity.X = direction * 28f;
            
            // Sound effect
            SoundEngine.PlaySound(SoundID.Item24 with { Pitch = 0.5f }, Player.Center);
            
            // VFX burst
            for (int i = 0; i < 25; i++)
            {
                float progress = i / 25f;
                Color dashColor = Color.Lerp(DeepPurple, Gold, progress);
                
                Vector2 pos = Player.Center + Main.rand.NextVector2Circular(30f, 20f);
                Vector2 vel = new Vector2(-direction * Main.rand.NextFloat(2f, 6f), Main.rand.NextFloat(-2f, 2f));
                
                var particle = new GenericGlowParticle(pos, vel, dashColor * 0.8f, 0.5f, 30, true);
                MagnumParticleHandler.SpawnParticle(particle);
            }
            
            // Star burst
            for (int i = 0; i < 12; i++)
            {
                float angle = MathHelper.TwoPi * i / 12f;
                Vector2 starPos = Player.Center + angle.ToRotationVector2() * 25f;
                CustomParticles.GenericFlare(starPos, StarWhite, 0.4f, 18);
            }
            
            // Constellation flash
            CustomParticles.GenericFlare(Player.Center, Gold, 0.8f, 20);
            CustomParticles.HaloRing(Player.Center, Violet, 0.6f, 15);
        }
    }
}
