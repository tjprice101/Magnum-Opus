using MagnumOpus.Common;
using MagnumOpus.Content.Eroica.Projectiles;
using Microsoft.Xna.Framework;
using System.Collections.Generic;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria;

namespace MagnumOpus.Content.Eroica.Weapons.FuneralPrayer
{
    /// <summary>
    /// Funeral Prayer — Eroica magic weapon channeling the solemn Marcia funebre (Funeral March).
    /// Launches slow funeral pyre projectiles that create persistent ground fires, with an Ash Requiem
    /// cone attack alt-fire and a Martyr's Exchange mechanic that empowers pyres when the player takes damage.
    /// Overlapping pyres merge into a devastating Eulogy pillar.
    /// </summary>
    public class FuneralPrayer : ModItem
    {
        /// <summary>
        /// Registers a beam hit for tracking ricochet chain mechanics.
        /// Called by FuneralPrayerBeam when it first hits an enemy.
        /// </summary>
        public static void RegisterBeamHit(int shotId, int beamIndex)
        {
            // Tracking hook for beam ricochet chain — can be expanded for damage escalation
        }

        public override void SetStaticDefaults()
        {
            Item.ResearchUnlockCount = 1;
        }

        public override void SetDefaults()
        {
            Item.ResearchUnlockCount = 1;
            Item.damage = 340;
            Item.DamageType = DamageClass.Magic;
            Item.width = 50;
            Item.height = 50;
            Item.useTime = 18;
            Item.useAnimation = 18;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.knockBack = 7f;
            Item.value = Item.sellPrice(gold: 35);
            Item.rare = ModContent.RarityType<EroicaRainbowRarity>();
            Item.UseSound = SoundID.Item20;
            Item.autoReuse = true;
            Item.shoot = ModContent.ProjectileType<FuneralPrayerProjectile>();
            Item.shootSpeed = 16f;
            Item.mana = 14;
            Item.noMelee = true;
            Item.maxStack = 1;
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "Effect1",
            "Launches funeral pyre projectiles that create lasting ground pyres"));
            tooltips.Add(new TooltipLine(Mod, "Effect2",
            "Ricochet beams chain between enemies, growing fiercer with each leap"));
            tooltips.Add(new TooltipLine(Mod, "Effect3",
            "Martyr's Exchange: taking damage empowers the next pyre"));
            tooltips.Add(new TooltipLine(Mod, "Lore",
            "'Even heroes kneel before the pyre.'")
            {
                OverrideColor = new Color(200, 50, 50)
            });
        }
    }
}
