using MagnumOpus.Common;
using MagnumOpus.Content.OdeToJoy.Weapons.RoseThornChainsaw.Projectiles;
using Microsoft.Xna.Framework;
using System.Collections.Generic;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria;
using MagnumOpus.Content.Fate.CraftingStations;
using MagnumOpus.Content.OdeToJoy.HarmonicCores;
using MagnumOpus.Content.OdeToJoy.ResonanceEnergies;

namespace MagnumOpus.Content.OdeToJoy.Weapons.RoseThornChainsaw
{
    /// <summary>
    /// Rose Thorn Chainsaw — pure botanical aggression.
    /// Continuous chainsaw with thorn fling every 0.5s.
    /// Petal Storm buildup at 4s of operation → 360° eruption.
    /// Enemies killed leave Rose Garden healing patches.
    /// </summary>
    public class RoseThornChainsaw : ModItem
    {
        public override void SetDefaults()
        {
            Item.ResearchUnlockCount = 1;
            Item.damage = 350; // Tier 9 (2100-3200 range), speed-proportional for useTime=1 chainsaw
            Item.DamageType = DamageClass.MeleeNoSpeed;
            Item.width = 54;
            Item.height = 24;
            Item.useTime = 1;
            Item.useAnimation = 1;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.channel = true;
            Item.noMelee = true;
            Item.noUseGraphic = true;
            Item.knockBack = 1f;
            Item.crit = 10;
            Item.autoReuse = true;
            Item.shoot = ModContent.ProjectileType<ChainsawHoldoutProjectile>();
            Item.shootSpeed = 32f;
            Item.UseSound = SoundID.Item22;
            Item.rare = ModContent.RarityType<OdeToJoyRarity>();
            Item.value = Item.sellPrice(platinum: 4);
        }

        public override bool CanUseItem(Player player)
        {
            // Only one holdout at a time
            int projType = ModContent.ProjectileType<ChainsawHoldoutProjectile>();
            for (int i = 0; i < Main.maxProjectiles; i++)
            {
                if (Main.projectile[i].active && Main.projectile[i].type == projType
                    && Main.projectile[i].owner == player.whoAmI)
                    return false;
            }
            return true;
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "Effect1",
                "Hold to run continuously — deals rapid contact damage"));
            tooltips.Add(new TooltipLine(Mod, "Effect2",
                "Flings thorn shrapnel at nearby enemies every 0.5 seconds"));
            tooltips.Add(new TooltipLine(Mod, "Effect3",
                "After 4 seconds of operation, erupts in a 360-degree Petal Storm"));
            tooltips.Add(new TooltipLine(Mod, "Effect4",
                "Holding longer increases rev speed — at max rev, +40% damage"));
            tooltips.Add(new TooltipLine(Mod, "Effect5",
                "Enemies killed leave Rose Garden patches that heal 2 HP/s"));
            tooltips.Add(new TooltipLine(Mod, "Lore",
                "'Every rose has its chainsaw.'")
            {
                OverrideColor = ChainsawTextures.LoreColor
            });
        }
    }
}