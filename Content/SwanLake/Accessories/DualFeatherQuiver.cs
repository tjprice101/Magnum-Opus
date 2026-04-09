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
    /// Dual Feather Quiver - Swan Lake ranged class accessory (Dual Mode).
    /// White Swan: 5% chance on ranged hit → Swan's Aria (5x damage on that shot).
    /// Black Swan: 10% chance on ranged hit → Swan's Opera (1.5x damage + 20% fire rate 3s).
    /// Both modes: 10% ammo conservation + 3% HP heal per saved shot.
    /// </summary>
    public class DualFeatherQuiver : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 32;
            Item.height = 32;
            Item.accessory = true;
            Item.value = Item.buyPrice(platinum: 3);
            Item.rare = ModContent.RarityType<SwanRarity>();
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            var attunement = player.GetModPlayer<MelodicAttunementPlayer>();
            attunement.resonantBurnDmgBonus += 0.60f;
            attunement.critDmgBonusOnBurn += 0.025f;
            attunement.rangedAttunement = true;

            var quiver = player.GetModPlayer<DualFeatherQuiverPlayer>();
            quiver.equipped = true;

            // Apply Swan's Opera fire rate bonus if active
            if (quiver.isBlackMode && quiver.swansOperaTimer > 0)
                player.GetAttackSpeed(DamageClass.Ranged) += 0.20f;

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
            var quiver = player.GetModPlayer<DualFeatherQuiverPlayer>();
            quiver.isBlackMode = !quiver.isBlackMode;
            Item.stack++;
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            var quiver = Main.LocalPlayer.GetModPlayer<DualFeatherQuiverPlayer>();
            Color lore = new Color(240, 240, 255);
            string mode = quiver.isBlackMode ? "Black Swan (Odile)" : "White Swan (Odette)";

            tooltips.Add(new TooltipLine(Mod, "Mode", $"Current: {mode} [Right-click to toggle]"));
            tooltips.Add(new TooltipLine(Mod, "Effect1", "'Resonance Pierced' Melodic Attunement"));
            tooltips.Add(new TooltipLine(Mod, "Effect2", "+60% increased Resonant Burn damage"));
            tooltips.Add(new TooltipLine(Mod, "Effect3", "Hitting an enemy 25 times with ranged damage while inflicted with Resonant Burn heals 10% HP"));
            tooltips.Add(new TooltipLine(Mod, "Effect4", "Critical strike damage on Resonant Burn enemies increased by 2.5%"));
            tooltips.Add(new TooltipLine(Mod, "Effect5", "10% chance to not consume ammo, heals 3% HP per saved shot"));
            tooltips.Add(new TooltipLine(Mod, "White", "White Swan: 5% chance on ranged hit for Swan's Aria (5x damage on that shot)"));
            tooltips.Add(new TooltipLine(Mod, "Black", "Black Swan: 10% chance on ranged hit for Swan's Opera (1.5x damage + 20% fire rate for 3s)"));
            tooltips.Add(new TooltipLine(Mod, "Effect6", "Immunity to fire debuffs, lava, confusion, and slow"));
            tooltips.Add(new TooltipLine(Mod, "Lore", "'Each feather remembers the flight -- one toward the light, one into darkness'")
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
                .AddIngredient(ItemID.SoulofSight, 5)
                .AddIngredient(ItemID.SoulofFlight, 8)
                .AddTile(ModContent.TileType<MoonlightAnvilTile>())
                .Register();
        }
    }

    public class DualFeatherQuiverPlayer : ModPlayer
    {
        public bool equipped = false;
        public bool isBlackMode = false;
        public int swansOperaTimer = 0;

        public override void ResetEffects()
        {
            equipped = false;
        }

        public override void PostUpdate()
        {
            if (swansOperaTimer > 0) swansOperaTimer--;
        }

        public override bool CanConsumeAmmo(Item weapon, Item ammo)
        {
            if (equipped && Main.rand.NextFloat() < 0.10f)
            {
                int heal = (int)(Player.statLifeMax2 * 0.03f);
                if (heal > 0)
                    Player.Heal(heal);
                return false;
            }
            return true;
        }

        public override void ModifyHitNPCWithProj(Projectile proj, NPC target, ref NPC.HitModifiers modifiers)
        {
            if (!equipped) return;
            if (!proj.DamageType.CountsAsClass(DamageClass.Ranged)) return;

            if (!isBlackMode)
            {
                // White: Swan's Aria — 5% chance for 5x damage on this hit
                if (Main.rand.NextFloat() < 0.05f)
                    modifiers.FinalDamage *= 5f;
            }
            else
            {
                // Black: Swan's Opera — 10% chance for 1.5x damage + 20% fire rate 3s
                if (Main.rand.NextFloat() < 0.10f)
                {
                    modifiers.FinalDamage *= 1.5f;
                    if (swansOperaTimer <= 0)
                        swansOperaTimer = 180; // 3 seconds
                }
            }
        }

        public override void ModifyHitNPCWithItem(Item item, NPC target, ref NPC.HitModifiers modifiers)
        {
            if (!equipped) return;
            if (!item.DamageType.CountsAsClass(DamageClass.Ranged)) return;

            if (!isBlackMode)
            {
                if (Main.rand.NextFloat() < 0.05f)
                    modifiers.FinalDamage *= 5f;
            }
            else
            {
                if (Main.rand.NextFloat() < 0.10f)
                {
                    modifiers.FinalDamage *= 1.5f;
                    if (swansOperaTimer <= 0)
                        swansOperaTimer = 180;
                }
            }
        }
    }
}
