using MagnumOpus.Common;
using MagnumOpus.Content.ClairDeLune.Weapons.StarfallWhisper.Projectiles;
using Microsoft.Xna.Framework;
using System.Collections.Generic;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace MagnumOpus.Content.ClairDeLune.Weapons.StarfallWhisper
{
    /// <summary>
    /// Starfall Whisper — Ranged bow that fires crystal arrows through time.
    /// Hits create Temporal Fractures (0.5s delayed replays dealing 40% damage).
    /// Alt fire: Shattered Time — 5 fracture arrows in spread. 3s cooldown.
    /// Arrows passing through Time Slow Fields refract into 2.
    /// </summary>
    public class StarfallWhisper : ModItem
    {
        private int _shatteredTimeCooldown;

        public override void SetDefaults()
        {
            Item.width = 44;
            Item.height = 76;
            Item.DamageType = DamageClass.Ranged;
            Item.damage = 3100; // Tier 10 (2800-4200 range)
            Item.useTime = 18;
            Item.useAnimation = 18;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.knockBack = 4f;
            Item.value = Item.sellPrice(platinum: 5);
            Item.rare = ModContent.RarityType<ClairDeLuneRarity>();
            Item.UseSound = SoundID.Item5;
            Item.autoReuse = true;
            Item.noMelee = true;
            Item.crit = 16;
            Item.shoot = ModContent.ProjectileType<TemporalArrowProjectile>();
            Item.shootSpeed = 18f;
            Item.useAmmo = AmmoID.Arrow;
        }

        public override bool AltFunctionUse(Player player)
        {
            return true;
        }

        public override bool CanUseItem(Player player)
        {
            if (player.altFunctionUse == 2)
            {
                if (_shatteredTimeCooldown > 0) return false;
                Item.useTime = 30;
                Item.useAnimation = 30;
            }
            else
            {
                Item.useTime = 18;
                Item.useAnimation = 18;
            }
            return base.CanUseItem(player);
        }

        public override void UpdateInventory(Player player)
        {
            if (_shatteredTimeCooldown > 0)
                _shatteredTimeCooldown--;
        }

        public override void HoldItem(Player player)
        {
            if (_shatteredTimeCooldown > 0)
                _shatteredTimeCooldown--;
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            Vector2 dir = velocity.SafeNormalize(Vector2.UnitX);

            if (player.altFunctionUse == 2)
            {
                // Shattered Time — 5 fracture arrows in 60° cone spread
                _shatteredTimeCooldown = 180; // 3s cooldown
                float totalSpread = MathHelper.ToRadians(60f);
                float angleStep = totalSpread / 4f;
                float startAngle = -totalSpread / 2f;

                for (int i = 0; i < 5; i++)
                {
                    float angle = startAngle + angleStep * i;
                    Vector2 shotDir = dir.RotatedBy(angle) * 18f;
                    Projectile.NewProjectile(source, position, shotDir,
                        ModContent.ProjectileType<TemporalArrowProjectile>(),
                        damage, knockback, player.whoAmI);
                }
                return false;
            }
            else
            {
                // Single crystal arrow
                Projectile.NewProjectile(source, position, dir * 18f,
                    ModContent.ProjectileType<TemporalArrowProjectile>(),
                    damage, knockback, player.whoAmI);
                return false;
            }
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "Effect1", "Fires crystal arrows that create Temporal Fractures on impact"));
            tooltips.Add(new TooltipLine(Mod, "Effect2", "Fractures replay the hit after a brief delay for bonus damage"));
            tooltips.Add(new TooltipLine(Mod, "Effect3", "Right click to fire 5 fracture arrows simultaneously"));
            tooltips.Add(new TooltipLine(Mod, "Effect4", "Nearby fractures resonate for chain reaction bursts"));
            tooltips.Add(new TooltipLine(Mod, "Lore", "'You hear the whisper only after the arrow has already arrived.'")
            {
                OverrideColor = ClairDeLunePalette.LoreText
            });
        }
    }
}
