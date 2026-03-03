using MagnumOpus.Common;
using MagnumOpus.Content.Eroica;
using Microsoft.Xna.Framework;
using System.Collections.Generic;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria;

namespace MagnumOpus.Content.Eroica.Weapons.CelestialValor
{
    public class CelestialValor : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 80;
            Item.height = 80;
            Item.damage = 320;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.useTime = 20;
            Item.useAnimation = 20;
            Item.useTurn = true;
            Item.DamageType = DamageClass.MeleeNoSpeed;
            Item.knockBack = 7.5f;
            Item.autoReuse = true;
            Item.noUseGraphic = true;
            Item.noMelee = true;
            Item.channel = true;
            Item.value = Item.sellPrice(gold: 45);
            Item.shoot = ModContent.ProjectileType<CelestialValorSwing>();
            Item.shootSpeed = 8f;
            Item.rare = ModContent.RarityType<EroicaRainbowRarity>();
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "Effect1",
            "Heroic Crescendo — 4-phase combo that builds to a devastating Finale Fortissimo"));
            tooltips.Add(new TooltipLine(Mod, "Effect2",
            "Combo spawns valor beams and culminates in a massive heroic detonation"));
            tooltips.Add(new TooltipLine(Mod, "Effect3",
            "Valor Gauge builds on successive hits — at maximum, Finale becomes Gloria"));
            tooltips.Add(new TooltipLine(Mod, "Effect4",
            "Hero's Resolve: below 30% HP, all swings deal 25% more damage")
            { OverrideColor = EroicaPalette.Gold });
            tooltips.Add(new TooltipLine(Mod, "Lore",
            "'To wield valor is to accept that every victory demands sacrifice.'")
            { OverrideColor = new Color(200, 50, 50) });
        }
    }
}
