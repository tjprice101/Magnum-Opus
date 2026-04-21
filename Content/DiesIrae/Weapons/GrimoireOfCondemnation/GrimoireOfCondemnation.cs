using MagnumOpus.Content.DiesIrae;
using MagnumOpus.Content.DiesIrae.Systems;
using Microsoft.Xna.Framework;
using System.Collections.Generic;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace MagnumOpus.Content.DiesIrae.Weapons.GrimoireOfCondemnation
{
    /// <summary>
    /// Grimoire of Condemnation — Dies Irae magic weapon.
    /// Left-click: Rapid fire blazing shards with mild homing.
    /// Every 7th cast: Page Turn — +30% damage.
    /// Right-click: Dark Sermon — summons ritual circle that detonates after 3s.
    /// Kills power next cast (+5% per condemned, max +50%).
    /// </summary>
    public class GrimoireOfCondemnation : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 32;
            Item.height = 32;
            Item.damage = 1650;
            Item.DamageType = DamageClass.Magic;
            Item.mana = 12;
            Item.useTime = 12;
            Item.useAnimation = 12;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.knockBack = 4f;
            Item.value = Item.sellPrice(platinum: 1, gold: 50);
            Item.rare = ModContent.RarityType<DiesIraeRarity>();
            Item.UseSound = SoundID.Item103;
            Item.autoReuse = true;
            Item.noMelee = true;
            Item.shoot = ModContent.ProjectileType<Projectiles.BlazingShardProjectile>();
            Item.shootSpeed = 16f;
            Item.crit = 15;
        }

        public override bool AltFunctionUse(Player player) => true;

        public override bool CanUseItem(Player player)
        {
            if (player.altFunctionUse == 2)
            {
                // Right-click: Ritual circle
                Item.useTime = 45;
                Item.useAnimation = 45;
                Item.mana = 40;
                Item.UseSound = SoundID.Item119 with { Pitch = -0.3f, Volume = 0.8f };
            }
            else
            {
                // Left-click: Normal rapid fire
                Item.useTime = 12;
                Item.useAnimation = 12;
                Item.mana = 12;
                Item.UseSound = SoundID.Item103;
            }
            return base.CanUseItem(player);
        }

        public override void ModifyShootStats(Player player, ref Vector2 position, ref Vector2 velocity, ref int type, ref int damage, ref float knockback)
        {
            if (player.altFunctionUse == 2)
            {
                // Ritual circle projectile
                type = ModContent.ProjectileType<Projectiles.RitualCircleProjectile>();
                damage = (int)(damage * 2.5f); // Ritual does 2.5x damage
            }
            else
            {
                // Apply kill bonus
                var combat = player.GetModPlayer<DiesIraeCombatPlayer>();
                if (combat.GrimoireKillBonus > 0)
                {
                    damage = (int)(damage * (1f + combat.GrimoireKillBonus));
                }
            }
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            if (player.altFunctionUse == 2)
            {
                // Spawn ritual circle at cursor
                Projectile.NewProjectile(source, Main.MouseWorld, Vector2.Zero, type, damage, knockback, player.whoAmI);
                return false;
            }

            // Normal blazing shard
            var combat = player.GetModPlayer<DiesIraeCombatPlayer>();
            combat.GrimoireCastCounter++;

            // Page Turn every 7th cast
            bool isPageTurn = combat.GrimoireCastCounter % 7 == 0;
            if (isPageTurn)
            {
                damage = (int)(damage * 1.3f);
                SoundEngine.PlaySound(SoundID.Item8 with { Pitch = 0.3f, Volume = 0.6f }, player.Center);
            }

            // Consume kill bonus shots
            if (combat.GrimoireKillBonusShots > 0)
                combat.GrimoireKillBonusShots--;

            // Spawn projectile with Page Turn and kill bonus info
            float killBonus = combat.GrimoireKillBonus;
            Projectile.NewProjectile(source, player.MountedCenter, velocity, type, damage, knockback, player.whoAmI,
                ai0: isPageTurn ? 1f : 0f,
                ai1: killBonus);

            // Fire 2 extra shards on Page Turn
            if (isPageTurn)
            {
                for (int i = 0; i < 2; i++)
                {
                    Vector2 offsetVel = velocity.RotatedBy((i - 0.5f) * 0.2f) * 0.9f;
                    Projectile.NewProjectile(source, player.MountedCenter, offsetVel, type, (int)(damage * 0.7f), knockback * 0.5f, player.whoAmI,
                        ai0: 0f, ai1: killBonus);
                }
            }

            return false;
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            Player player = Main.LocalPlayer;
            var combat = player.GetModPlayer<DiesIraeCombatPlayer>();
            int castCount = combat.GrimoireCastCounter;
            int untilPageTurn = 7 - (castCount % 7);
            float killBonus = combat.GrimoireKillBonus;

            string castDisplay = untilPageTurn == 7 ? "[c/C8AA32:PAGE TURN READY]" : $"Casts until Page Turn: {untilPageTurn}";
            tooltips.Add(new TooltipLine(Mod, "CastCounter", castDisplay));

            if (killBonus > 0)
            {
                string bonusText = $"[c/FF8844:Condemned Bonus: +{(int)(killBonus * 100)}%] ({combat.GrimoireKillBonusShots} shots)";
                tooltips.Add(new TooltipLine(Mod, "KillBonus", bonusText));
            }

            tooltips.Add(new TooltipLine(Mod, "Effect1", "Rapid fire blazing shards with mild homing"));
            tooltips.Add(new TooltipLine(Mod, "Effect2", "Every 7th cast: Page Turn fires 3 empowered shards"));
            tooltips.Add(new TooltipLine(Mod, "Effect3", "Right-click: Dark Sermon summons a ritual circle that detonates"));
            tooltips.Add(new TooltipLine(Mod, "Effect4", "Kills power next 5 shots (+5% per condemned, max +50%)"));
            tooltips.Add(new TooltipLine(Mod, "Lore", "'Every name written in this book burns twice — once on the page, once in flesh.'")
            {
                OverrideColor = DiesIraePalette.LoreText
            });
        }
    }
}
