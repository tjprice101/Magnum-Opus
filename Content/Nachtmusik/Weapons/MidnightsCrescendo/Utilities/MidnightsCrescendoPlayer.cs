using System;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using MagnumOpus.Common.Systems.UI;
using MagnumOpus.Common.Utilities;

namespace MagnumOpus.Content.Nachtmusik.Weapons.MidnightsCrescendo.Utilities
{
    public class MidnightsCrescendoPlayer : ModPlayer, IResonantOverdrive
    {
        private int _overdriveStarRainTimer;

        // === Charge Meter ===
        public float Charge = 0f;
        public const float ChargePerHit = 0.05f;
        public const float MaxCharge = 1.0f;
        public bool IsHoldingMidnightsCrescendo = false;
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
            IsHoldingMidnightsCrescendo = false;
        }

        public override void OnHitNPCWithProj(Projectile proj, NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (proj.type == ModContent.ProjectileType<Projectiles.MidnightsCrescendoSwing>())
                AddCharge(target.life <= 0 ? 0.15f : ChargePerHit);
        }

        #region IResonantOverdrive

        bool IResonantOverdrive.IsHoldingOverdriveWeapon => IsHoldingMidnightsCrescendo;
        float IResonantOverdrive.OverdriveCharge => Charge;
        bool IResonantOverdrive.IsOverdriveReady => IsChargeFull;
        Color IResonantOverdrive.OverdriveLowColor => new Color(65, 60, 150);
        Color IResonantOverdrive.OverdriveHighColor => new Color(165, 190, 255);

        bool IResonantOverdrive.ActivateOverdrive(Player player)
        {
            if (player.whoAmI != Main.myPlayer) return true;
            _overdriveStarRainTimer = 480;
            ConsumeCharge();
            return true;
        }

        #endregion

        public override void PostUpdate()
        {
            if (_overdriveStarRainTimer > 0)
            {
                _overdriveStarRainTimer--;
                if (_overdriveStarRainTimer % 6 == 0 && Player.whoAmI == Main.myPlayer)
                {
                    int damage = Math.Max(1, (int)(Player.HeldItem.damage * 1.25f));
                    Vector2 spawn = new Vector2(Player.Center.X + Main.rand.NextFloat(-950f, 950f), Player.Center.Y - Main.rand.NextFloat(500f, 900f));
                    Vector2 velocity = new Vector2(Main.rand.NextFloat(-1.2f, 1.2f), Main.rand.NextFloat(11f, 15f));
                    Projectile.NewProjectile(Player.GetSource_FromThis(), spawn, velocity, ProjectileID.Starfury, damage, 3f, Player.whoAmI);
                }
            }
        }
    }

    public static class MidnightsCrescendoPlayerExtensions
    {
        public static MidnightsCrescendoPlayer MidnightsCrescendo(this Player player)
            => player.GetModPlayer<MidnightsCrescendoPlayer>();
    }
}
