using MagnumOpus.Common;
using MagnumOpus.Content.ClairDeLune.Weapons.Chronologicality.Projectiles;
using MagnumOpus.Content.ClairDeLune.Weapons.Chronologicality.Utilities;
using Microsoft.Xna.Framework;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace MagnumOpus.Content.ClairDeLune.Weapons.Chronologicality
{
    /// <summary>
    /// Chronologicality — Temporal Broadsword (3-Phase Clock Combo)
    /// A broadsword forged from crystallized time. Three-phase combo mimics the three
    /// hands of a clock: Hour Hand (heavy, slow cleave), Minute Hand (mid sweep),
    /// Second Hand (rapid flurry). Temporal echoes replay damage, Time Slow Fields
    /// linger at impacts, and Clockwork Overflow triggers after a perfect 3-phase cycle.
    /// </summary>
    public class Chronologicality : ModItem
    {
        public override void SetDefaults()
        {
            Item.DamageType = DamageClass.MeleeNoSpeed;
            Item.damage = 285;
            Item.width = 58;
            Item.height = 28;
            Item.useTime = 1;
            Item.useAnimation = 1;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.channel = true;
            Item.noMelee = true;
            Item.noUseGraphic = true;
            Item.knockBack = 6f;
            Item.value = Item.sellPrice(platinum: 5);
            Item.rare = ModContent.RarityType<ClairDeLuneRarity>();
            Item.UseSound = SoundID.Item71;
            Item.autoReuse = true;
            Item.shoot = ModContent.ProjectileType<ChronologicalitySwing>();
            Item.shootSpeed = 45f;
            Item.crit = 18;
        }

        public override bool AltFunctionUse(Player player) => true;

        public override bool CanShoot(Player player)
        {
            // Prevent overlapping swing projectiles
            return !Main.projectile.Any(p =>
                p.active && p.owner == player.whoAmI &&
                p.type == ModContent.ProjectileType<ChronologicalitySwing>());
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source,
            Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            var mp = player.ChronologicalityState();
            bool isOverflow = player.altFunctionUse == 2 && mp.CanTriggerOverflow;

            // ai[0] = combo phase (0=Hour, 1=Minute, 2=Second), ai[1] = overflow flag
            float comboPhase = mp.ComboPhase;
            float overflowFlag = isOverflow ? 1f : 0f;

            Projectile.NewProjectile(source, position, velocity, type,
                damage, knockback, player.whoAmI, comboPhase, overflowFlag);

            return false;
        }

        public override void HoldItem(Player player)
        {
            var mp = player.ChronologicalityState();
            mp.HoldingWeapon = true;
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "Effect1",
                "Three-phase clock-hand combo: Hour, Minute, Second"));
            tooltips.Add(new TooltipLine(Mod, "Effect2",
                "Each swing leaves temporal echoes that replay 30% damage after a brief delay"));
            tooltips.Add(new TooltipLine(Mod, "Effect3",
                "Hit enemies leave Time Slow Fields that slow nearby foes for 3 seconds"));
            tooltips.Add(new TooltipLine(Mod, "Effect4",
                "Right-click after a perfect 3-phase cycle triggers Clockwork Overflow"));
            tooltips.Add(new TooltipLine(Mod, "Lore",
                "'Every swing is a second spent. Every combo is a minute passing. And when the hour strikes — time itself holds its breath.'")
            {
                OverrideColor = ClairDeLunePalette.LoreText
            });
        }
    }
}
