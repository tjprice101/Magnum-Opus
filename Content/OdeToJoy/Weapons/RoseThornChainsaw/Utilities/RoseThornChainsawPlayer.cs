using System;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;
using MagnumOpus.Common.Systems.UI;

namespace MagnumOpus.Content.OdeToJoy.Weapons.RoseThornChainsaw.Utilities
{
    public class RoseThornChainsawPlayer : ModPlayer, IResonantOverdrive
    {
        // === Charge Meter ===
        public float Charge = 0f;
        public const float ChargePerHit = 0.05f;
        public const float MaxCharge = 1.0f;
        public bool IsHoldingRoseThornChainsaw = false;
        public bool IsChargeFull => Charge >= MaxCharge;

        // === Projectile Empower ===
        public int ProjectileEmpowerTimer;
        public bool IsEmpowerActive => ProjectileEmpowerTimer > 0;

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
            IsHoldingRoseThornChainsaw = false;
        }

        public override void PostUpdate()
        {
            if (ProjectileEmpowerTimer > 0)
                ProjectileEmpowerTimer--;
        }

        public override void OnHitNPCWithProj(Projectile proj, NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (proj.type == ModContent.ProjectileType<Projectiles.RoseThornChainsawProjectile>())
                AddCharge(target.life <= 0 ? 0.15f : ChargePerHit);
        }

        // === IResonantOverdrive ===
        bool IResonantOverdrive.IsHoldingOverdriveWeapon => IsHoldingRoseThornChainsaw;
        float IResonantOverdrive.OverdriveCharge => Charge;
        bool IResonantOverdrive.IsOverdriveReady => IsChargeFull;
        Color IResonantOverdrive.OverdriveLowColor => new Color(170, 55, 85);
        Color IResonantOverdrive.OverdriveHighColor => new Color(255, 170, 200);

        bool IResonantOverdrive.ActivateOverdrive(Player player)
        {
            if (player.whoAmI != Main.myPlayer)
                return true;

            ProjectileEmpowerTimer = 600;
            CombatText.NewText(player.Hitbox, Color.LightPink, "Chainsaw Overdrive", true);

            ConsumeCharge();
            return true;
        }
    }

    public static class RoseThornChainsawPlayerExtensions
    {
        public static RoseThornChainsawPlayer RoseThornChainsaw(this Player player)
            => player.GetModPlayer<RoseThornChainsawPlayer>();
    }
}
