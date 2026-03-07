using MagnumOpus.Common;
using Microsoft.Xna.Framework;
using System.Collections.Generic;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria;
using MagnumOpus.Content.DiesIrae.HarmonicCores;
using MagnumOpus.Content.DiesIrae.ResonanceEnergies;
using MagnumOpus.Content.Fate.CraftingStations;

namespace MagnumOpus.Content.DiesIrae.Weapons.StaffOfFinalJudgement
{
    public class StaffOfFinalJudgement : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 48;
            Item.height = 48;
            Item.damage = 1950;
            Item.DamageType = DamageClass.Magic;
            Item.mana = 22;
            Item.useTime = 30;
            Item.useAnimation = 30;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.knockBack = 5f;
            Item.value = Item.sellPrice(platinum: 2);
            Item.rare = ModContent.RarityType<global::MagnumOpus.Common.DiesIraeRarity>();
            Item.UseSound = SoundID.Item117;
            Item.autoReuse = true;
            Item.noMelee = true;
            Item.shoot = ModContent.ProjectileType<Projectiles.FloatingIgnitionProjectile>();
            Item.shootSpeed = 4f;
            Item.crit = 18;
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "Effect1", "Places floating fire mines that auto-arm after 1 second"));
            tooltips.Add(new TooltipLine(Mod, "Effect2", "Armed mines detonate when enemies approach within 3 tiles"));
            tooltips.Add(new TooltipLine(Mod, "Effect3", "Adjacent mines chain-detonate with +30% bonus damage"));
            tooltips.Add(new TooltipLine(Mod, "Effect4", "3+ mines detonating within 1 second triggers Judgment Storm — massive fire rain"));
            tooltips.Add(new TooltipLine(Mod, "Lore", "'Judgment does not chase. Judgment waits.'")
            {
                OverrideColor = new Color(200, 50, 30)
            });
        }
    }
}