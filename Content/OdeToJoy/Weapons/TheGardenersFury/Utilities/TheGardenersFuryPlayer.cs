using System;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using MagnumOpus.Common.Systems.UI;
using MagnumOpus.Common.Utilities;

namespace MagnumOpus.Content.OdeToJoy.Weapons.TheGardenersFury.Utilities
{
    public class TheGardenersFuryPlayer : ModPlayer, IResonantOverdrive
    {
        // Seeds planted grow fury; fury level escalates the gardener's wrath
        public int seedsPlanted;
        public int furyLevel;
        public bool isActive;
        public int activeTimer;

        // === Charge Meter ===
        public float Charge = 0f;
        public const float ChargePerHit = 0.05f;
        public const float MaxCharge = 1.0f;
        public bool IsHoldingTheGardenersFury = false;
        public bool IsChargeFull => Charge >= MaxCharge;

        public void AddCharge(float amount)
        {
            Charge = System.Math.Clamp(Charge + amount, 0f, MaxCharge);
        }

        public void ConsumeCharge()
        {
            Charge = 0f;
        }

        public override void ResetEffects()
        {
            IsHoldingTheGardenersFury = false;
            if (!isActive)
            {
                if (activeTimer > 0)
                    activeTimer--;
                if (activeTimer <= 0)
                {
                    seedsPlanted = 0;
                    furyLevel = 0;
                }
            }
            isActive = false;
        }

        public void PlantSeed()
        {
            seedsPlanted++;
            activeTimer = 150;
        }

        public void AddFury(int amount = 1)
        {
            furyLevel = System.Math.Min(furyLevel + amount, 10);
            activeTimer = 120;
        }

        public void ConsumeSeeds()
        {
            seedsPlanted = 0;
        }

        public float GetFuryIntensity()
        {
            return furyLevel / 10f;
        }

        public override void OnHitNPCWithProj(Projectile proj, NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (proj.type == Terraria.ModLoader.ModContent.ProjectileType<Projectiles.GardenerFuryProjectile>())
                AddCharge(target.life <= 0 ? 0.15f : ChargePerHit);
        }

        // === IResonantOverdrive ===
        bool IResonantOverdrive.IsHoldingOverdriveWeapon => IsHoldingTheGardenersFury;
        float IResonantOverdrive.OverdriveCharge => Charge;
        bool IResonantOverdrive.IsOverdriveReady => IsChargeFull;
        Color IResonantOverdrive.OverdriveLowColor => new Color(95, 150, 70);
        Color IResonantOverdrive.OverdriveHighColor => new Color(230, 255, 170);

        bool IResonantOverdrive.ActivateOverdrive(Player player)
        {
            if (player.whoAmI != Main.myPlayer)
                return true;

            int baseDamage = Math.Max(1, player.HeldItem.damage);
            foreach (NPC npc in NpcTargetingUtils.EnumerateHostiles(player.Center, 1300f))
            {
                Vector2 spawn = npc.Center + new Vector2(Main.rand.NextFloat(-90f, 90f), -700f);
                Vector2 vel = (npc.Center - spawn).SafeNormalize(Vector2.UnitY) * 16f;
                Projectile.NewProjectile(player.GetSource_FromThis(), spawn, vel, ProjectileID.HallowStar, baseDamage * 3, 3f, player.whoAmI);
            }

            ConsumeCharge();
            return true;
        }
    }

    public static class TheGardenersFuryPlayerExtensions
    {
        public static TheGardenersFuryPlayer TheGardenersFury(this Player player)
            => player.GetModPlayer<TheGardenersFuryPlayer>();
    }
}
