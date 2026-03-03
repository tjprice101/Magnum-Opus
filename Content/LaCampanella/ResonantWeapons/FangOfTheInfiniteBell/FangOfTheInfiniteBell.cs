using Microsoft.Xna.Framework;
using System.Collections.Generic;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using MagnumOpus.Content.LaCampanella.ResonantWeapons.FangOfTheInfiniteBell.Projectiles;
using MagnumOpus.Content.LaCampanella.ResonantWeapons.FangOfTheInfiniteBell.Utilities;
using MagnumOpus.Common;

namespace MagnumOpus.Content.LaCampanella.ResonantWeapons.FangOfTheInfiniteBell
{
    /// <summary>
    /// Fang of the Infinite Bell — La Campanella magic staff.
    /// "Infinity is not a destination; it is a bell that rings without ceasing."
    /// 
    /// Primary: Launches bell-shaped energy orbs that bounce between enemies 2 times.
    ///          Each bounce spawns a smaller echo orb (half damage, 1 bounce).
    ///          Each successful bounce grants +3% magic damage (max 20 stacks = +60%).
    /// At 10+ stacks: Lightning arcs between airborne orbs.
    /// At 20 stacks: Orbs explode on final bounce.
    /// Alt-fire at 20 stacks: Infinite Crescendo — giant orb, 10 bounces, 4 echo orbs per bounce.
    /// </summary>
    public class FangOfTheInfiniteBell : ModItem
    {
        public override string Texture => "MagnumOpus/Content/LaCampanella/ResonantWeapons/FangOfTheInfiniteBell/FangOfTheInfiniteBell";

        public override void SetDefaults()
        {
            Item.width = 40;
            Item.height = 40;
            Item.damage = 95;
            Item.DamageType = DamageClass.Magic;
            Item.mana = 12;
            Item.useTime = 18;
            Item.useAnimation = 18;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.knockBack = 3f;
            Item.crit = 8;
            Item.value = Item.sellPrice(gold: 50);
            Item.rare = ModContent.RarityType<LaCampanellaRainbowRarity>();
            Item.autoReuse = true;
            Item.noMelee = true;
            Item.shoot = ModContent.ProjectileType<InfiniteBellOrbProj>();
            Item.shootSpeed = 10f;
            Item.channel = false;
        }

        public override bool AltFunctionUse(Player player) => true;

        public override bool CanUseItem(Player player)
        {
            if (player.altFunctionUse == 2)
            {
                // Alt-fire: Infinite Crescendo requires max stacks (20)
                var fbPlayer = player.FangOfTheInfiniteBell();
                return fbPlayer.CanInfiniteCrescendo;
            }
            return base.CanUseItem(player);
        }

        public override void ModifyShootStats(Player player, ref Vector2 position, ref Vector2 velocity, ref int type, ref int damage, ref float knockback)
        {
            if (player.altFunctionUse == 2)
            {
                // Infinite Crescendo: giant orb, 10 bounces, full damage
                type = ModContent.ProjectileType<InfiniteBellOrbProj>();
                damage = (int)(damage * 1.5f); // 150% base damage for Crescendo
            }
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            if (player.altFunctionUse == 2)
            {
                // Infinite Crescendo: consume all stacks, fire giant orb
                var fbPlayer = player.FangOfTheInfiniteBell();
                fbPlayer.ConsumeAllStacks();

                // ai[0] = max bounces (10 for Crescendo)
                // ai[1] = isCrescendo flag (1 = Crescendo orb)
                int proj = Projectile.NewProjectile(source, position, velocity * 0.7f, type, damage, knockback, player.whoAmI, 10f, 1f);
                return false;
            }

            // Normal fire: bell orb with 2 bounces
            // ai[0] = max bounces (2 for normal), ai[1] = isCrescendo (0)
            Projectile.NewProjectile(source, position, velocity, type, damage, knockback, player.whoAmI, 2f, 0f);
            return false;
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "Effect1", "Launches bell orbs that bounce between enemies"));
            tooltips.Add(new TooltipLine(Mod, "Effect2", "Each bounce grants stacking magic damage (max +60%)"));
            tooltips.Add(new TooltipLine(Mod, "Effect3", "At 10+ stacks, lightning arcs between airborne orbs"));
            tooltips.Add(new TooltipLine(Mod, "Effect4", "Right click at max stacks for Infinite Crescendo"));
            tooltips.Add(new TooltipLine(Mod, "Lore", "'Infinity is not a destination; it is a bell that rings without ceasing.'")
            {
                OverrideColor = FangOfTheInfiniteBellUtils.LoreColor
            });
        }
    }
}
