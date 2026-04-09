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
    /// Crown of the Swan - Swan Lake magic class accessory (Dual Mode).
    /// White Swan: 10% chance on magic hit → Glorious Swan (next 5 magic casts consume no mana).
    /// Black Swan: 10% chance on magic hit → Swan of the Black Flame (next 5 magic attacks deal double damage).
    /// Both modes: -15% mana cost, 4% mana refund on hit.
    /// </summary>
    public class CrownOfTheSwan : ModItem
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
            attunement.magicAttunement = true;

            var crown = player.GetModPlayer<CrownOfTheSwanPlayer>();
            crown.equipped = true;

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
            var crown = player.GetModPlayer<CrownOfTheSwanPlayer>();
            crown.isBlackMode = !crown.isBlackMode;
            Item.stack++;
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            var crown = Main.LocalPlayer.GetModPlayer<CrownOfTheSwanPlayer>();
            Color lore = new Color(240, 240, 255);
            string mode = crown.isBlackMode ? "Black Swan (Odile)" : "White Swan (Odette)";

            tooltips.Add(new TooltipLine(Mod, "Mode", $"Current: {mode} [Right-click to toggle]"));
            tooltips.Add(new TooltipLine(Mod, "Effect1", "'Resonance Seared' Melodic Attunement"));
            tooltips.Add(new TooltipLine(Mod, "Effect2", "+60% increased Resonant Burn damage"));
            tooltips.Add(new TooltipLine(Mod, "Effect3", "Hitting an enemy 15 times with magic damage while inflicted with Resonant Burn heals 10% HP"));
            tooltips.Add(new TooltipLine(Mod, "Effect4", "Critical strike damage on Resonant Burn enemies increased by 2.5%"));
            tooltips.Add(new TooltipLine(Mod, "Effect5", "-15% mana cost, 4% mana refund on hit"));
            tooltips.Add(new TooltipLine(Mod, "White", "White Swan: 10% chance on magic hit for Glorious Swan (next 5 casts consume no mana)"));
            tooltips.Add(new TooltipLine(Mod, "Black", "Black Swan: 10% chance on magic hit for Swan of the Black Flame (next 5 attacks deal double damage)"));
            tooltips.Add(new TooltipLine(Mod, "Effect6", "Immunity to fire debuffs, lava, confusion, and slow"));
            tooltips.Add(new TooltipLine(Mod, "Lore", "'Worn by royalty who understood that true power lies in choice'")
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
                .AddIngredient(ItemID.SoulofLight, 5)
                .AddIngredient(ItemID.SoulofFlight, 6)
                .AddTile(ModContent.TileType<MoonlightAnvilTile>())
                .Register();
        }
    }

    public class CrownOfTheSwanPlayer : ModPlayer
    {
        public bool equipped = false;
        public bool isBlackMode = false;
        public int gloriousSwanCasts = 0; // White: free mana casts remaining
        public int blackFlameCasts = 0;   // Black: double damage casts remaining

        public override void ResetEffects()
        {
            equipped = false;
        }

        public override void ModifyManaCost(Item item, ref float reduce, ref float mult)
        {
            if (!equipped) return;
            if (!item.DamageType.CountsAsClass(DamageClass.Magic)) return;

            // Base -15% mana cost
            mult *= 0.85f;

            // White mode: Glorious Swan — free casts
            if (!isBlackMode && gloriousSwanCasts > 0)
            {
                mult = 0f;
                gloriousSwanCasts--;
            }
        }

        public override void OnHitNPCWithProj(Projectile proj, NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (!equipped) return;
            if (!proj.DamageType.CountsAsClass(DamageClass.Magic)) return;
            HandleMagicHit();
        }

        public override void OnHitNPCWithItem(Item item, NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (!equipped) return;
            if (!item.DamageType.CountsAsClass(DamageClass.Magic)) return;
            HandleMagicHit();
        }

        private void HandleMagicHit()
        {
            // 4% mana refund on hit
            int manaRefund = (int)(Player.statManaMax2 * 0.04f);
            if (manaRefund > 0)
            {
                Player.statMana += manaRefund;
                if (Player.statMana > Player.statManaMax2)
                    Player.statMana = Player.statManaMax2;
            }

            // Mode-specific procs
            if (!isBlackMode)
            {
                // White: 10% chance for Glorious Swan
                if (Main.rand.NextFloat() < 0.10f && gloriousSwanCasts <= 0)
                    gloriousSwanCasts = 5;
            }
            else
            {
                // Black: 10% chance for Swan of the Black Flame
                if (Main.rand.NextFloat() < 0.10f && blackFlameCasts <= 0)
                    blackFlameCasts = 5;
            }
        }

        public override void ModifyHitNPCWithProj(Projectile proj, NPC target, ref NPC.HitModifiers modifiers)
        {
            if (!equipped) return;
            if (!proj.DamageType.CountsAsClass(DamageClass.Magic)) return;

            // Black mode: Swan of the Black Flame — double damage
            if (isBlackMode && blackFlameCasts > 0)
            {
                modifiers.FinalDamage *= 2f;
                blackFlameCasts--;
            }
        }

        public override void ModifyHitNPCWithItem(Item item, NPC target, ref NPC.HitModifiers modifiers)
        {
            if (!equipped) return;
            if (!item.DamageType.CountsAsClass(DamageClass.Magic)) return;

            if (isBlackMode && blackFlameCasts > 0)
            {
                modifiers.FinalDamage *= 2f;
                blackFlameCasts--;
            }
        }
    }
}
