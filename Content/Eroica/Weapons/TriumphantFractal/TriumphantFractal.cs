using MagnumOpus.Common;
using MagnumOpus.Content.Eroica;
using MagnumOpus.Content.Eroica.Projectiles;
using Microsoft.Xna.Framework;
using System.Collections.Generic;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria;

namespace MagnumOpus.Content.Eroica.Weapons.TriumphantFractal
{
    /// <summary>
    /// Triumphant Fractal — Eroica magic weapon manifesting heroism as self-similar fractal patterns.
    /// Fires bolts that recursively split into smaller copies on impact (2 generations of 3-way splits),
    /// with overlapping fragments creating Resonance Zone AoE damage patches. Features a Fractal Shield
    /// alt-fire that absorbs projectiles and a Triumph Accumulator that unleashes a 64-fragment barrage.
    /// </summary>
    public class TriumphantFractal : ModItem
    {
        public override void SetStaticDefaults()
        {
            Item.ResearchUnlockCount = 1;
        }

        public override void SetDefaults()
        {
            Item.ResearchUnlockCount = 1;
            Item.damage = 490; // Tier 2 (300-500 range)
            Item.DamageType = DamageClass.Magic;
            Item.width = 56;
            Item.height = 56;
            Item.scale = 0.075f;
            Item.useTime = 25;
            Item.useAnimation = 25;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.knockBack = 8f;
            Item.value = Item.sellPrice(gold: 45);
            Item.rare = ModContent.RarityType<EroicaRainbowRarity>();
            Item.UseSound = SoundID.Item43;
            Item.autoReuse = true;
            Item.shoot = ModContent.ProjectileType<TriumphantFractalProjectile>();
            Item.shootSpeed = 14f;
            Item.mana = 19;
            Item.noMelee = true;
            Item.maxStack = 1;
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "Effect1",
            "Fires homing fractal bolts that split into recursive generations on impact")
            { OverrideColor = EroicaPalette.Gold });
            tooltips.Add(new TooltipLine(Mod, "Effect2",
            "Overlapping fragments create Resonance Zones — brief AoE damage patches"));
            tooltips.Add(new TooltipLine(Mod, "Effect3",
            "After 10 fractal kills, next cast fires a Triumphant Fractal with 64 fragments"));
            tooltips.Add(new TooltipLine(Mod, "Effect4",
            "Fractal Shield alt-fire absorbs projectiles and converts them to fractal energy"));
            tooltips.Add(new TooltipLine(Mod, "Lore",
            "'In every fragment, the whole persists.'")
            {
                OverrideColor = new Color(200, 50, 50)
            });
        }
    }
}
