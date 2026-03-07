using MagnumOpus.Common;
using MagnumOpus.Content.DiesIrae.Weapons.GrimoireOfCondemnation.Projectiles;
using Microsoft.Xna.Framework;
using System.Collections.Generic;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria;
using MagnumOpus.Content.DiesIrae.HarmonicCores;
using MagnumOpus.Content.DiesIrae.ResonanceEnergies;
using MagnumOpus.Content.Fate.CraftingStations;

namespace MagnumOpus.Content.DiesIrae.Weapons.GrimoireOfCondemnation
{
    public class GrimoireOfCondemnation : ModItem
    {
        private int castCount = 0;

        public override void SetDefaults()
        {
            Item.width = 32;
            Item.height = 32;
            Item.damage = 1650; // Tier 8 (1600-2400 range)
            Item.DamageType = DamageClass.Magic;
            Item.mana = 12;
            Item.useTime = 15;
            Item.useAnimation = 15;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.knockBack = 4f;
            Item.value = Item.sellPrice(platinum: 1, gold: 50);
            Item.rare = ModContent.RarityType<global::MagnumOpus.Common.DiesIraeRarity>();
            Item.UseSound = SoundID.Item103;
            Item.autoReuse = true;
            Item.noMelee = true;
            Item.channel = true;
            Item.shoot = ModContent.ProjectileType<BlazingShardProjectile>();
            Item.shootSpeed = 12f;
            Item.crit = 15;
        }

        public override bool AltFunctionUse(Player player) => true;

        public override bool CanUseItem(Player player)
        {
            if (player.altFunctionUse == 2)
            {
                // Dark Sermon — higher mana cost
                Item.mana = 40;
                Item.useTime = 30;
                Item.useAnimation = 30;
                Item.channel = false;
            }
            else
            {
                Item.mana = 12;
                Item.useTime = 15;
                Item.useAnimation = 15;
                Item.channel = true;
            }
            return base.CanUseItem(player);
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            if (player.altFunctionUse == 2)
            {
                // Dark Sermon — ritual circle at cursor position
                Vector2 targetPos = Main.MouseWorld;
                Projectile.NewProjectile(source, targetPos, Vector2.Zero,
                    ModContent.ProjectileType<DarkSermonSigilProjectile>(),
                    (int)(damage * 2.5f), knockback, player.whoAmI);
                return false;
            }

            // Condemnation Wave beam
            castCount++;
            int actualDamage = damage;

            // Page Turn: every 7th cast = +30% damage
            if (castCount % 7 == 0)
            {
                actualDamage = (int)(damage * 1.3f);
                Utilities.GrimoireOfCondemnationUtils.DoPageTurn(player.Center);
            }

            Projectile.NewProjectile(source, position, velocity, type, actualDamage, knockback, player.whoAmI);
            return false;
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "Effect1", "Channels a sweepable Condemnation Wave beam that widens over time"));
            tooltips.Add(new TooltipLine(Mod, "Effect2", "Every 7th cast: Page Turn — beam deals +30% damage"));
            tooltips.Add(new TooltipLine(Mod, "Effect3", "Right-click: Dark Sermon — summons a ritual circle that detonates after 3 seconds"));
            tooltips.Add(new TooltipLine(Mod, "Effect4", "Kills power next cast (+5% per condemned, max +50%)"));
            tooltips.Add(new TooltipLine(Mod, "Lore", "'Every name written in this book burns twice — once on the page, once in flesh.'")
            {
                OverrideColor = new Color(200, 50, 30)
            });
        }
    }
}