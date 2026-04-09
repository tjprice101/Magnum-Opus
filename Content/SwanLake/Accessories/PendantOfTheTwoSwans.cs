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
    /// Pendant of the Two Swans - Swan Lake melee class accessory (Dual Mode).
    /// White Swan (Odette): 5% chance on melee hit → Odette's Wonder (+5% dmg 5s).
    /// Black Swan (Odile): 5% chance on melee hit → Odile's Grace (+25% melee speed 3s).
    /// </summary>
    public class PendantOfTheTwoSwans : ModItem
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
            attunement.meleeAttunement = true;

            var pendant = player.GetModPlayer<PendantOfTheTwoSwansPlayer>();
            pendant.equipped = true;

            // Apply mode-specific bonuses
            if (pendant.isBlackMode && pendant.odilesGraceTimer > 0)
                player.GetAttackSpeed(DamageClass.Melee) += 0.25f;
            if (!pendant.isBlackMode && pendant.odettesWonderTimer > 0)
                player.GetDamage(DamageClass.Generic) += 0.05f;

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
            var pendant = player.GetModPlayer<PendantOfTheTwoSwansPlayer>();
            pendant.isBlackMode = !pendant.isBlackMode;
            Item.stack++;
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            var pendant = Main.LocalPlayer.GetModPlayer<PendantOfTheTwoSwansPlayer>();
            Color lore = new Color(240, 240, 255);
            string mode = pendant.isBlackMode ? "Black Swan (Odile)" : "White Swan (Odette)";

            tooltips.Add(new TooltipLine(Mod, "Mode", $"Current: {mode} [Right-click to toggle]"));
            tooltips.Add(new TooltipLine(Mod, "Effect1", "'Resonance Sliced' Melodic Attunement"));
            tooltips.Add(new TooltipLine(Mod, "Effect2", "+60% increased Resonant Burn damage"));
            tooltips.Add(new TooltipLine(Mod, "Effect3", "Hitting an enemy 10 times with melee damage while inflicted with Resonant Burn heals 10% HP"));
            tooltips.Add(new TooltipLine(Mod, "Effect4", "Critical strike damage on Resonant Burn enemies increased by 2.5%"));
            tooltips.Add(new TooltipLine(Mod, "White", "White Swan: 5% chance on melee hit for Odette's Wonder (+5% damage for 5s)"));
            tooltips.Add(new TooltipLine(Mod, "Black", "Black Swan: 5% chance on melee hit for Odile's Grace (+25% melee speed for 3s)"));
            tooltips.Add(new TooltipLine(Mod, "Effect5", "Immunity to fire debuffs, lava, confusion, and slow"));
            tooltips.Add(new TooltipLine(Mod, "Lore", "'Two souls entwined in eternal dance -- light and shadow, love and deception'")
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
                .AddIngredient(ItemID.SoulofMight, 5)
                .AddIngredient(ItemID.SoulofFlight, 10)
                .AddTile(ModContent.TileType<MoonlightAnvilTile>())
                .Register();
        }
    }

    public class PendantOfTheTwoSwansPlayer : ModPlayer
    {
        public bool equipped = false;
        public bool isBlackMode = false;
        public int odettesWonderTimer = 0;
        public int odilesGraceTimer = 0;

        public override void ResetEffects()
        {
            equipped = false;
        }

        public override void PostUpdate()
        {
            if (odettesWonderTimer > 0) odettesWonderTimer--;
            if (odilesGraceTimer > 0) odilesGraceTimer--;
        }

        public override void OnHitNPCWithItem(Item item, NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (!equipped) return;
            if (!item.DamageType.CountsAsClass(DamageClass.Melee)) return;
            TryProc();
        }

        public override void OnHitNPCWithProj(Projectile proj, NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (!equipped) return;
            if (!proj.DamageType.CountsAsClass(DamageClass.Melee)) return;
            TryProc();
        }

        private void TryProc()
        {
            if (Main.rand.NextFloat() < 0.05f)
            {
                if (!isBlackMode && odettesWonderTimer <= 0)
                    odettesWonderTimer = 300; // 5 seconds
                else if (isBlackMode && odilesGraceTimer <= 0)
                    odilesGraceTimer = 180; // 3 seconds
            }
        }
    }
}
