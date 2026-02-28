using System;
using Terraria;
using Terraria.ModLoader;
using MagnumOpus.Content.OdeToJoy.Weapons.TheGardenersFury.Projectiles;

namespace MagnumOpus.Content.OdeToJoy.Weapons.TheGardenersFury.Utilities
{
    /// <summary>
    /// ModPlayer that tracks combo stacks for The Gardener's Fury rapier.
    /// +1 stack per hit (max 10), resets after 90 frames without hitting.
    /// Grants +5% melee attack speed per stack.
    /// </summary>
    public class ComboStackPlayer : ModPlayer
    {
        /// <summary>
        /// Current combo stacks (0–10).
        /// </summary>
        public int ComboStacks;

        /// <summary>
        /// Frames remaining before combo resets. Set to 90 on each hit.
        /// </summary>
        public int ComboTimer;

        /// <summary>
        /// Maximum combo stacks.
        /// </summary>
        public const int MaxStacks = 10;

        /// <summary>
        /// Frames of inactivity before combo resets.
        /// </summary>
        public const int ComboTimeout = 90;

        public override void PostUpdateEquips()
        {
            // Grant +5% melee attack speed per combo stack
            if (ComboStacks > 0)
                Player.GetAttackSpeed(DamageClass.Melee) += 0.05f * ComboStacks;
        }

        public override void PostUpdate()
        {
            // Decay combo timer
            if (ComboTimer > 0)
            {
                ComboTimer--;
            }
            else if (ComboStacks > 0)
            {
                // Timer expired — reset stacks
                ComboStacks = 0;
            }
        }

        /// <summary>
        /// Called by the rapier projectile on hit — increment stacks and reset timer.
        /// Also called via OnHitNPCWithProj as a safety net.
        /// </summary>
        public void RegisterHit()
        {
            ComboStacks = Math.Min(ComboStacks + 1, MaxStacks);
            ComboTimer = ComboTimeout;
        }

        public override void OnHitNPCWithProj(Projectile proj, NPC target, NPC.HitInfo hit, int damageDone)
        {
            // Only count hits from the Gardener's Fury rapier thrust projectile
            if (proj.type == ModContent.ProjectileType<GardenerFuryProjectile>())
            {
                RegisterHit();
            }
        }
    }
}
