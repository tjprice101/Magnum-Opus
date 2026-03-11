using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using MagnumOpus.Common;
using MagnumOpus.Content.Fate;
using MagnumOpus.Content.Fate.ResonanceEnergies;
using MagnumOpus.Content.Fate.HarmonicCores;
using MagnumOpus.Content.MoonlightSonata.CraftingStations;

namespace MagnumOpus.Content.Fate.Accessories
{
    /// <summary>
    /// Machination of the Event Horizon - Movement/Utility accessory for Fate theme.
    /// Combines the agility of ninja gear with cosmic evasion powers.
    /// Grants enhanced dash, wall climb, and the ability to phase through attacks briefly.
    /// </summary>
    public class MachinationoftheEventHorizon : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 36;
            Item.height = 36;
            Item.accessory = true;
            Item.value = Item.buyPrice(platinum: 3, gold: 50);
            Item.rare = ModContent.RarityType<FateRarity>();
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            var modPlayer = player.GetModPlayer<EventHorizonPlayer>();
            modPlayer.hasEventHorizon = true;
            
            // Master Ninja Gear effects
            player.dashType = 1; // Ninja dash
            player.spikedBoots = 2; // Tiger climbing gear
            player.blackBelt = true; // Chance to dodge
            
            // Terraspark Boots effects
            player.accRunSpeed = 6.75f;
            player.rocketBoots = player.vanityRocketBoots = 3;
            player.moveSpeed += 0.08f;
            player.iceSkate = true;
            player.waterWalk = true;
            player.fireWalk = true;
            player.lavaMax += 7 * 60;
            
            // Frog Leg effects
            player.autoJump = true;
            player.jumpSpeedBoost += 2.4f;
            player.fallStart = (int)(player.fallStart + player.maxFallSpeed);
            
            // Cosmic Dodge Chance bonus
            player.GetModPlayer<EventHorizonPlayer>().cosmicDodgeChance += 0.08f;
        }

        public override void ModifyTooltips(System.Collections.Generic.List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "NinjaEffects", "Grants the ability to dash, wall climb, and auto-dodge")
            {
                OverrideColor = new Color(200, 150, 255)
            });

            tooltips.Add(new TooltipLine(Mod, "SpeedEffects", "Provides the speed and mobility of Terraspark Boots")
            {
                OverrideColor = new Color(255, 200, 100)
            });

            tooltips.Add(new TooltipLine(Mod, "JumpEffects", "Grants enhanced jump height from Frog Leg")
            {
                OverrideColor = new Color(150, 255, 150)
            });

            tooltips.Add(new TooltipLine(Mod, "CosmicDodge", "8% chance to phase through attacks into the void")
            {
                OverrideColor = FatePalette.DarkPink
            });

            tooltips.Add(new TooltipLine(Mod, "Flavor", "'Space bends around those who walk the event horizon'")
            {
                OverrideColor = FatePalette.FatePurple
            });
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ModContent.ItemType<FateResonantEnergy>(), 15)
                .AddIngredient(ModContent.ItemType<ResonantCoreOfFate>(), 5)
                .AddIngredient(ItemID.MasterNinjaGear, 1)
                .AddIngredient(ItemID.TerrasparkBoots, 1)
                .AddIngredient(ItemID.FrogLeg, 1)
                .AddIngredient(ItemID.LunarBar, 10)
                .AddTile(ModContent.TileType<MoonlightAnvilTile>())
                .Register();
        }
    }

    public class EventHorizonPlayer : ModPlayer
    {
        public bool hasEventHorizon = false;
        public float cosmicDodgeChance = 0f;
        private int phaseCooldown = 0;

        public override void ResetEffects()
        {
            hasEventHorizon = false;
            cosmicDodgeChance = 0f;
        }

        public override void PostUpdate()
        {
            if (phaseCooldown > 0)
                phaseCooldown--;
        }

        public override bool FreeDodge(Player.HurtInfo info)
        {
            if (hasEventHorizon && cosmicDodgeChance > 0 && phaseCooldown <= 0)
            {
                if (Main.rand.NextFloat() < cosmicDodgeChance)
                {
                    // Cosmic phase VFX
                    TriggerPhaseVFX();
                    phaseCooldown = 180; // 3 second cooldown on cosmic dodge
                    return true;
                }
            }
            return false;
        }

        private void TriggerPhaseVFX()
        {
            FateAccessoryVFX.EventHorizonPhaseVFX(Player.Center);
        }
    }
}
