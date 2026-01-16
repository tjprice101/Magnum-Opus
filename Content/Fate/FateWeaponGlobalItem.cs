using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.DataStructures;
using MagnumOpus.Common;

namespace MagnumOpus.Content.Fate
{
    /// <summary>
    /// Global item that applies the Fate Astrological Ring effect to all Fate-rarity weapons.
    /// When any Fate weapon attacks, it has a chance to trigger the kaleidoscopic ring attack.
    /// </summary>
    public class FateWeaponGlobalItem : GlobalItem
    {
        // Cooldown tracking per player
        private static int[] fateRingCooldown = new int[Main.maxPlayers];
        
        // Cooldown between ring spawns (in ticks)
        private const int RingCooldown = 90; // 1.5 seconds
        
        // Chance to trigger on attack (percentage)
        private const int TriggerChance = 100; // 100% for now, can adjust

        public override bool InstancePerEntity => false;

        public override void Load()
        {
            // Initialize cooldown array
            for (int i = 0; i < Main.maxPlayers; i++)
            {
                fateRingCooldown[i] = 0;
            }
        }

        /// <summary>
        /// Decrements cooldowns each tick
        /// </summary>
        public static void UpdateCooldowns()
        {
            for (int i = 0; i < Main.maxPlayers; i++)
            {
                if (fateRingCooldown[i] > 0)
                    fateRingCooldown[i]--;
            }
        }

        /// <summary>
        /// Checks if an item is a Fate-rarity weapon
        /// </summary>
        private bool IsFateWeapon(Item item)
        {
            if (item == null || item.IsAir) return false;
            
            // Check if it's a Fate rarity item
            return item.rare == ModContent.RarityType<FateRarity>();
        }

        /// <summary>
        /// Trigger the astrological ring effect
        /// </summary>
        private void TriggerFateRing(Player player, Item item, int damage)
        {
            if (fateRingCooldown[player.whoAmI] > 0) return;
            
            // Random chance check
            if (Main.rand.Next(100) >= TriggerChance) return;
            
            // Set cooldown
            fateRingCooldown[player.whoAmI] = RingCooldown;
            
            // Spawn the astrological ring projectile
            int ringDamage = (int)(damage * 1.5f); // 150% of weapon damage
            
            Projectile.NewProjectile(
                player.GetSource_ItemUse(item),
                player.Center,
                Vector2.Zero,
                ModContent.ProjectileType<FateAstrologicalRing>(),
                ringDamage,
                8f,
                player.whoAmI
            );
        }

        /// <summary>
        /// Hook for melee weapons - trigger on swing
        /// </summary>
        public override void UseItemHitbox(Item item, Player player, ref Rectangle hitbox, ref bool noHitbox)
        {
            if (!IsFateWeapon(item)) return;
            if (item.DamageType != DamageClass.Melee && !item.DamageType.CountsAsClass(DamageClass.Melee)) return;
            
            // Only trigger at the start of the swing
            if (player.itemAnimation == player.itemAnimationMax - 1)
            {
                TriggerFateRing(player, item, item.damage);
            }
        }

        /// <summary>
        /// Hook for ranged/magic/summon weapons - trigger on shoot
        /// </summary>
        public override bool Shoot(Item item, Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            if (!IsFateWeapon(item)) return true;
            
            // Only trigger for non-melee weapons (melee is handled in UseItemHitbox)
            if (item.DamageType == DamageClass.Melee || item.DamageType.CountsAsClass(DamageClass.Melee))
            {
                // Still trigger for melee weapons that shoot projectiles
                if (player.itemAnimation == player.itemAnimationMax - 1)
                {
                    TriggerFateRing(player, item, damage);
                }
                return true;
            }
            
            // For ranged/magic/summon
            TriggerFateRing(player, item, damage);
            
            return true;
        }
    }

    /// <summary>
    /// ModSystem to update Fate ring cooldowns
    /// </summary>
    public class FateWeaponSystem : ModSystem
    {
        public override void PostUpdatePlayers()
        {
            FateWeaponGlobalItem.UpdateCooldowns();
        }
    }
}
