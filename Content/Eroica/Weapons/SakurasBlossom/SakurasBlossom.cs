using MagnumOpus.Common;
using MagnumOpus.Content.Eroica;
using Microsoft.Xna.Framework;
using System.Collections.Generic;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria;

namespace MagnumOpus.Content.Eroica.Weapons.SakurasBlossom
{
    public class SakurasBlossom : ModItem
    {
        public override void SetDefaults()
        {
            Item.ResearchUnlockCount = 1;
            Item.width = 70;
            Item.height = 70;
            Item.damage = 350;
            Item.DamageType = DamageClass.MeleeNoSpeed;
            Item.useTime = 20;
            Item.useAnimation = 20;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.knockBack = 8f;
            Item.scale = 1.3f;
            Item.value = Item.sellPrice(gold: 40);
            Item.rare = ModContent.RarityType<EroicaRainbowRarity>();
            Item.UseSound = null;
            Item.autoReuse = true;
            Item.channel = true;
            Item.noMelee = true;
            Item.noUseGraphic = true;
            Item.shoot = ModContent.ProjectileType<SakurasBlossomSwing>();
            Item.shootSpeed = 6f;
            Item.maxStack = 1;
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "Effect1",
            "Petal Dance — 3-phase flowing combo scatters sakura petals and homing spectral copies")
            { OverrideColor = new Color(255, 180, 200) });
            tooltips.Add(new TooltipLine(Mod, "Effect2",
            "Final Bloom unleashes a 360-degree petal burst that converges on struck enemies"));
            tooltips.Add(new TooltipLine(Mod, "Effect3",
            "Sakura Meditation: hold without nearby enemies for enhanced next swing")
            { OverrideColor = EroicaPalette.Gold });
            tooltips.Add(new TooltipLine(Mod, "Lore",
            "'Every petal that falls is a promise kept.'")
            { OverrideColor = new Color(200, 50, 50) });
        }
    }
}
