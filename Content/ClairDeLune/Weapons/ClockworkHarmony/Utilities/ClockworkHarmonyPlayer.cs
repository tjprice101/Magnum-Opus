using System;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using MagnumOpus.Common.Systems.UI;
using MagnumOpus.Common.Utilities;

namespace MagnumOpus.Content.ClairDeLune.Weapons.ClockworkHarmony.Utilities
{
    public class ClockworkHarmonyPlayer : ModPlayer, IResonantOverdrive
    {
        private int _overdriveBombTimer;

        // === Charge Meter ===
        public float Charge = 0f;
        public const float ChargePerHit = 0.05f;
        public const float MaxCharge = 1.0f;
        public bool IsHoldingClockworkHarmony = false;
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
            IsHoldingClockworkHarmony = false;
        }

        public override void OnHitNPCWithProj(Projectile proj, NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (proj.type == ModContent.ProjectileType<Projectiles.ClockworkHarmonySwing>())
                AddCharge(target.life <= 0 ? 0.15f : ChargePerHit);
        }

        #region IResonantOverdrive

        bool IResonantOverdrive.IsHoldingOverdriveWeapon => IsHoldingClockworkHarmony;
        float IResonantOverdrive.OverdriveCharge => Charge;
        bool IResonantOverdrive.IsOverdriveReady => IsChargeFull;
        Color IResonantOverdrive.OverdriveLowColor => new Color(150, 120, 60);
        Color IResonantOverdrive.OverdriveHighColor => new Color(255, 210, 120);

        bool IResonantOverdrive.ActivateOverdrive(Player player)
        {
            if (player.whoAmI != Main.myPlayer) return true;
            _overdriveBombTimer = 90;
            ConsumeCharge();
            return true;
        }

        #endregion

        public override void PostUpdate()
        {
            if (_overdriveBombTimer > 0)
            {
                _overdriveBombTimer--;
                if (_overdriveBombTimer % 6 == 0 && Player.whoAmI == Main.myPlayer)
                {
                    int damage = Math.Max(1, Player.HeldItem.damage * 5);
                    float x = Player.Center.X + Main.rand.NextFloat(-900f, 900f);
                    Vector2 spawn = new Vector2(x, Player.Center.Y - Main.rand.NextFloat(550f, 850f));
                    Vector2 velocity = new Vector2(Main.rand.NextFloat(-2f, 2f), Main.rand.NextFloat(9f, 14f));
                    int proj = Projectile.NewProjectile(Player.GetSource_FromThis(), spawn, velocity, ProjectileID.Bomb, damage, 4f, Player.whoAmI);
                    if (proj >= 0 && proj < Main.maxProjectiles)
                        Main.projectile[proj].timeLeft = Main.rand.Next(30, 100);
                }
            }
        }
    }

    public static class ClockworkHarmonyPlayerExtensions
    {
        public static ClockworkHarmonyPlayer ClockworkHarmony(this Player player)
            => player.GetModPlayer<ClockworkHarmonyPlayer>();
    }
}
