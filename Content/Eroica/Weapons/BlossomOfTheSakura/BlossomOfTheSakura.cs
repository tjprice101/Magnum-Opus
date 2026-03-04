using MagnumOpus.Common;
using MagnumOpus.Content.Eroica.Projectiles;
using MagnumOpus.Content.Eroica;
using Microsoft.Xna.Framework;
using System.Collections.Generic;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria;

namespace MagnumOpus.Content.Eroica.Weapons.BlossomOfTheSakura
{
    /// <summary>
    /// Blossom of the Sakura — Eroica ranged weapon that rains sakura arrows from afar.
    /// Arrows bloom into petal explosions on contact, with a charged Petal Storm volley alt-fire
    /// and homing Tracer Blossom shots that mark targets for bonus damage.
    /// </summary>
    public class BlossomOfTheSakura : ModItem
    {
        public override void SetStaticDefaults()
        {
            Item.ResearchUnlockCount = 1;
        }

        public override void SetDefaults()
        {
            Item.ResearchUnlockCount = 1;
            Item.damage = 75;
            Item.DamageType = DamageClass.Ranged;
            Item.width = 64;
            Item.height = 28;
            Item.useTime = 4;
            Item.useAnimation = 4;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.knockBack = 3f;
            Item.value = Item.sellPrice(gold: 38);
            Item.rare = ModContent.RarityType<EroicaRainbowRarity>();
            Item.UseSound = SoundID.Item11;
            Item.autoReuse = true;
            Item.shoot = ModContent.ProjectileType<BlossomOfTheSakuraBulletProjectile>();
            Item.shootSpeed = 18f;
            Item.useAmmo = AmmoID.Bullet;
            Item.noMelee = true;
            Item.maxStack = 1;
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "Effect1",
            "High fire-rate blossom arrows that explode into petal bursts on contact")
            { OverrideColor = EroicaPalette.Sakura });
            tooltips.Add(new TooltipLine(Mod, "Effect2",
            "Sustained fire heats the barrel — hotter shots track harder and glow brighter")
            { OverrideColor = new Color(240, 180, 100) });
            tooltips.Add(new TooltipLine(Mod, "Effect3",
            "Every 5th shot fires a Tracer Blossom that marks targets for 10% bonus damage")
            { OverrideColor = EroicaPalette.Gold });
            tooltips.Add(new TooltipLine(Mod, "Lore",
            "'Every bullet carries a petal. Every petal, a prayer.'")
            { OverrideColor = EroicaPalette.Scarlet });
        }
    }
}
