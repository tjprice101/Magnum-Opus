using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace MagnumOpus.Common.BaseClasses
{
    /// <summary>
    /// Abstract base for ALL Calamity-style held-projectile melee ITEMS.
    /// Extracted from InfernalCleaverItem's pattern.
    ///
    /// Handles:
    ///   channel + noMelee + noUseGraphic setup,
    ///   combo step tracking with reset timer,
    ///   active-swing gate (CanShoot),
    ///   MountedCenter positioning,
    ///   projectile spawning with ai[0] = comboStep.
    ///
    /// Subclasses MUST define:
    ///   SwingProjectileType  — the ModProjectile type ID for the swing
    ///   ComboStepCount       — total number of combo phases
    ///
    /// Subclasses MAY override:
    ///   ComboResetDelay, GetLoreColor(), SetWeaponDefaults(),
    ///   AddWeaponTooltips(), OnShoot()
    /// </summary>
    public abstract class MeleeSwingItemBase : ModItem
    {
        #region Abstract Members — MUST override

        /// <summary>The ProjectileType&lt;T&gt;() of the swing projectile.</summary>
        protected abstract int SwingProjectileType { get; }

        /// <summary>How many combo phases the weapon has (typically 4).</summary>
        protected abstract int ComboStepCount { get; }

        #endregion

        #region Virtual Members — MAY override

        /// <summary>Frames of inactivity before combo resets to step 0. Default 45.</summary>
        protected virtual int ComboResetDelay => 45;

        /// <summary>Color for the lore tooltip line. Override per-theme.</summary>
        protected virtual Color GetLoreColor() => new Color(200, 200, 200);

        /// <summary>
        /// Set weapon-specific defaults (damage, useTime, rare, etc.).
        /// Called at the END of SetDefaults after the base configures
        /// channel/noMelee/noUseGraphic/shoot.
        /// </summary>
        protected virtual void SetWeaponDefaults() { }

        /// <summary>
        /// Add weapon-specific tooltip lines (effect descriptions + lore).
        /// Called inside ModifyTooltips.
        /// </summary>
        protected virtual void AddWeaponTooltips(List<TooltipLine> tooltips) { }

        /// <summary>
        /// Called after the swing projectile has been spawned.
        /// Use for extra effects on shoot (sounds, particles, etc.).
        /// Return the projectile index if you want to modify it further.
        /// </summary>
        protected virtual void OnShoot(Player player, int projectileIndex) { }

        #endregion

        #region Combo Tracking

        private int comboStep;
        private int comboResetTimer;

        /// <summary>Current combo step (read-only for subclasses).</summary>
        public int CurrentComboStep => comboStep;

        #endregion

        #region SetDefaults — Base Configuration

        public sealed override void SetDefaults()
        {
            // Held-projectile pattern (Calamity Exoblade standard)
            Item.useStyle = ItemUseStyleID.Swing;
            Item.noMelee = true;
            Item.noUseGraphic = true;
            Item.channel = true;

            Item.DamageType = DamageClass.MeleeNoSpeed;
            Item.shoot = SwingProjectileType;
            Item.shootSpeed = 1f;

            // Reasonable defaults — subclass overrides in SetWeaponDefaults()
            Item.width = 80;
            Item.height = 80;
            Item.useTime = 30;
            Item.useAnimation = 30;
            Item.autoReuse = true;

            // Let the subclass set damage, rare, value, etc.
            SetWeaponDefaults();
        }

        #endregion

        #region HoldItem — Combo Reset Timer

        public override void HoldItem(Player player)
        {
            if (comboResetTimer > 0)
            {
                comboResetTimer--;
                if (comboResetTimer <= 0)
                    comboStep = 0;
            }
        }

        #endregion

        #region CanShoot — Prevent Overlapping Swings

        public override bool CanShoot(Player player)
        {
            int shootType = SwingProjectileType;

            for (int i = 0; i < Main.maxProjectiles; i++)
            {
                Projectile p = Main.projectile[i];
                if (p.active && p.owner == player.whoAmI && p.type == shootType)
                {
                    // Allow new swing if current one is in post-swing stasis
                    if (p.ModProjectile is MeleeSwingBase swing && swing.InPostSwingStasis)
                        continue;

                    return false;
                }
            }

            return true;
        }

        #endregion

        #region ModifyShootStats — MountedCenter Positioning

        public override void ModifyShootStats(Player player, ref Vector2 position,
            ref Vector2 velocity, ref int type, ref int damage, ref float knockback)
        {
            position = player.MountedCenter;
            velocity = player.MountedCenter.DirectionTo(Main.MouseWorld);
        }

        #endregion

        #region Shoot — Spawn Swing Projectile with Combo Step

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source,
            Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            int proj = Projectile.NewProjectile(
                source, position, velocity, type, damage, knockback,
                player.whoAmI, ai0: comboStep);

            // Advance combo
            comboStep = (comboStep + 1) % ComboStepCount;
            comboResetTimer = ComboResetDelay;

            OnShoot(player, proj);

            return false;
        }

        #endregion

        #region ModifyTooltips — Subclass Tooltips

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            AddWeaponTooltips(tooltips);
        }

        #endregion
    }
}
