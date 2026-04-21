using MagnumOpus.Common.Systems;
using MagnumOpus.Content.ClairDeLune;
using MagnumOpus.Content.ClairDeLune.Weapons.MidnightMechanism.Projectiles;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace MagnumOpus.Content.ClairDeLune.Weapons.MidnightMechanism
{
    public class MidnightMechanism : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 72;
            Item.height = 36;
            Item.damage = 2900;
            Item.DamageType = DamageClass.Ranged;
            Item.useAnimation = 20;
            Item.useTime = 20;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.knockBack = 2f;
            Item.value = Item.sellPrice(platinum: 5);
            Item.rare = ModContent.RarityType<ClairDeLuneRarity>();
            Item.UseSound = SoundID.Item11;
            Item.autoReuse = true;
            Item.noMelee = true;
            Item.shoot = ModContent.ProjectileType<MechanismBulletProjectile>();
            Item.shootSpeed = 22f;
            Item.useAmmo = AmmoID.Bullet;
            Item.crit = 16;
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source,
            Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            var combatPlayer = player.GetModPlayer<ClairDeLuneCombatPlayer>();

            // Spin-Up Gatling: escalate fire rate through 5 phases
            // Phase 1: 20f useTime (normal)
            // Phase 2: 12f useTime
            // Phase 3: 8f useTime
            // Phase 4: 5f useTime
            // Phase 5: 3f useTime + homing

            combatPlayer.MidnightMechanismTimer = 180; // 3 seconds before phase decay
            if (combatPlayer.MidnightMechanismPhase < 5)
                combatPlayer.MidnightMechanismPhase++;

            combatPlayer.MidnightMechanismBulletCount++;

            // Every 12th bullet in phase 5 = Midnight Strike (2x scale, 10x damage)
            bool isMidnightStrike = combatPlayer.MidnightMechanismPhase == 5 &&
                combatPlayer.MidnightMechanismBulletCount % 12 == 0;

            Projectile.NewProjectile(source, position, velocity, type,
                isMidnightStrike ? damage * 10 : damage,
                isMidnightStrike ? knockback * 2f : knockback, player.whoAmI,
                ai0: combatPlayer.MidnightMechanismPhase, ai1: isMidnightStrike ? 1f : 0f);
            return false;
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "Lore", "'The clock does not care if you are ready. Midnight comes regardless.'")
            {
                OverrideColor = ClairDeLunePalette.LoreText
            });
        }
    }
}
