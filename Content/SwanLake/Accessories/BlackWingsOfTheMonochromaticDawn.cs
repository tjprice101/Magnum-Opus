using Microsoft.Xna.Framework;
using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using MagnumOpus.Common;
using MagnumOpus.Content.Common.Accessories;
using MagnumOpus.Content.SwanLake.ResonanceEnergies;
using MagnumOpus.Content.SwanLake.HarmonicCores;
using MagnumOpus.Content.MoonlightSonata.CraftingStations;

namespace MagnumOpus.Content.SwanLake.Accessories
{
    /// <summary>
    /// Black Wings of the Monochromatic Dawn - Swan Lake summoner class accessory (Dual Mode).
    /// White Swan: 5% on minion hit → heal player 1% HP.
    /// Black Swan: 15% on minion hit → 10% DR for 2s.
    /// Both modes: +10% summon damage, +3 minion slots.
    /// </summary>
    public class BlackWingsOfTheMonochromaticDawn : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 32;
            Item.height = 32;
            Item.accessory = true;
            Item.value = Item.buyPrice(platinum: 3);
            Item.rare = ModContent.RarityType<SwanRainbowRarity>();
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            var attunement = player.GetModPlayer<MelodicAttunementPlayer>();
            attunement.resonantBurnDmgBonus += 0.60f;
            attunement.critDmgAll += 0.20f;
            attunement.summonAttunement = true;

            var wings = player.GetModPlayer<BlackWingsPlayer>();
            wings.equipped = true;

            // DR if active
            if (wings.isBlackMode && wings.drTimer > 0)
                player.endurance += 0.10f;

            // +10% summon damage + 3 minion slots
            player.GetDamage(DamageClass.Summon) += 0.10f;
            player.maxMinions += 3;

            // Fire/lava/confusion/slow immunity
            player.buffImmune[BuffID.OnFire] = true;
            player.buffImmune[BuffID.OnFire3] = true;
            player.lavaImmune = true;
            player.buffImmune[BuffID.Confused] = true;
            player.buffImmune[BuffID.Slow] = true;
        }

        public override bool CanRightClick() => true;

        public override void RightClick(Player player)
        {
            var wings = player.GetModPlayer<BlackWingsPlayer>();
            wings.isBlackMode = !wings.isBlackMode;
            Item.stack++;
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            var wings = Main.LocalPlayer.GetModPlayer<BlackWingsPlayer>();
            Color lore = new Color(240, 240, 255);
            string mode = wings.isBlackMode ? "Black Swan (Odile)" : "White Swan (Odette)";

            tooltips.Add(new TooltipLine(Mod, "Mode", $"Current: {mode} [Right-click to toggle]"));
            tooltips.Add(new TooltipLine(Mod, "Effect1", "'Resonance Born' Melodic Attunement"));
            tooltips.Add(new TooltipLine(Mod, "Effect2", "+60% increased Resonant Burn damage"));
            tooltips.Add(new TooltipLine(Mod, "Effect3", "Hitting an enemy 30 times with summon damage while inflicted with Resonant Burn heals 10% HP"));
            tooltips.Add(new TooltipLine(Mod, "Effect4", "+20% universal critical strike damage"));
            tooltips.Add(new TooltipLine(Mod, "Effect5", "+10% summon damage, +3 minion slots"));
            tooltips.Add(new TooltipLine(Mod, "White", "White Swan: 5% on minion hit to heal 1% max HP"));
            tooltips.Add(new TooltipLine(Mod, "Black", "Black Swan: 15% on minion hit for 10% damage reduction for 2s"));
            tooltips.Add(new TooltipLine(Mod, "Effect6", "Immunity to fire debuffs, lava, confusion, and slow"));
            tooltips.Add(new TooltipLine(Mod, "Lore", "'At dawn, when darkness meets light, the monochromatic wings spread -- neither fully black nor white, but eternally both'")
            {
                OverrideColor = lore
            });
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ModContent.ItemType<SwansResonanceEnergy>(), 5)
                .AddIngredient(ModContent.ItemType<ResonantCoreOfSwanLake>(), 5)
                .AddIngredient(ModContent.ItemType<RemnantOfSwansHarmony>(), 5)
                .AddIngredient(ModContent.ItemType<ShardOfTheFeatheredTempo>(), 5)
                .AddIngredient(ItemID.SoulofNight, 5)
                .AddIngredient(ItemID.SoulofFlight, 15)
                .AddTile(ModContent.TileType<MoonlightAnvilTile>())
                .Register();
        }
    }

    public class BlackWingsPlayer : ModPlayer
    {
        public bool equipped = false;
        public bool isBlackMode = false;
        public int drTimer = 0;

        public override void ResetEffects()
        {
            equipped = false;
        }

        public override void PostUpdate()
        {
            if (drTimer > 0) drTimer--;
        }

        public override void OnHitNPCWithProj(Projectile proj, NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (!equipped) return;
            bool isSummon = proj.minion || proj.sentry ||
                            proj.DamageType.CountsAsClass(DamageClass.Summon);
            if (!isSummon) return;

            if (!isBlackMode)
            {
                // White: 5% chance to heal 1% HP
                if (Main.rand.NextFloat() < 0.05f)
                {
                    int heal = (int)(Player.statLifeMax2 * 0.01f);
                    if (heal > 0)
                        Player.Heal(heal);
                }
            }
            else
            {
                // Black: 15% chance for 10% DR for 2s
                if (Main.rand.NextFloat() < 0.15f && drTimer <= 0)
                    drTimer = 120; // 2 seconds
            }
        }
    }
}
