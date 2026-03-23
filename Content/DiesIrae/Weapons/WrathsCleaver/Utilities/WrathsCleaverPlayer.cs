using System;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using MagnumOpus.Common.Systems.UI;
using MagnumOpus.Common.Utilities;

namespace MagnumOpus.Content.DiesIrae.Weapons.WrathsCleaver.Utilities
{
    public class WrathsCleaverPlayer : ModPlayer, IResonantOverdrive
    {
        private int _overdriveBarrageTimer;

        // Wrath stacks build from 0-5 as the cleaver feeds on fury
        public int wrathStacks;
        public int comboCounter;
        public bool isActive;
        public int activeTimer;

        // === Charge Meter ===
        public float Charge = 0f;
        public const float ChargePerHit = 0.05f;
        public const float MaxCharge = 1.0f;
        public bool IsHoldingWrathsCleaver = false;
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
            IsHoldingWrathsCleaver = false;

            if (!isActive)
            {
                if (activeTimer > 0)
                    activeTimer--;
                if (activeTimer <= 0)
                {
                    wrathStacks = 0;
                    comboCounter = 0;
                }
            }
            isActive = false;
        }

        public void IncrementCombo()
        {
            comboCounter = (comboCounter + 1) % 4;
            activeTimer = 60;
        }

        public void AddWrath(int amount = 1)
        {
            wrathStacks = System.Math.Min(wrathStacks + amount, 5);
            activeTimer = 120;
        }

        public void ConsumeWrath()
        {
            wrathStacks = 0;
        }

        public float GetWrathIntensity()
        {
            return wrathStacks / 5f;
        }

        public override void OnHitNPCWithProj(Projectile proj, NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (proj.type == ModContent.ProjectileType<Projectiles.WrathsCleaverSwing>())
                AddCharge(target.life <= 0 ? 0.15f : ChargePerHit);
        }

        #region IResonantOverdrive

        bool IResonantOverdrive.IsHoldingOverdriveWeapon => IsHoldingWrathsCleaver;
        float IResonantOverdrive.OverdriveCharge => Charge;
        bool IResonantOverdrive.IsOverdriveReady => IsChargeFull;
        Color IResonantOverdrive.OverdriveLowColor => new Color(180, 40, 30);
        Color IResonantOverdrive.OverdriveHighColor => new Color(255, 140, 70);

        bool IResonantOverdrive.ActivateOverdrive(Player player)
        {
            if (player.whoAmI != Main.myPlayer) return true;
            _overdriveBarrageTimer = 40;
            ConsumeCharge();
            return true;
        }

        #endregion

        public override void PostUpdate()
        {
            if (_overdriveBarrageTimer > 0)
            {
                _overdriveBarrageTimer--;
                if (_overdriveBarrageTimer % 8 == 0 && Player.whoAmI == Main.myPlayer)
                {
                    int damage = Math.Max(1, (int)(Player.HeldItem.damage * 1.3f));
                    for (int i = 0; i < 12; i++)
                    {
                        Vector2 vel = (MathHelper.TwoPi * i / 12f).ToRotationVector2() * Main.rand.NextFloat(8f, 11f);
                        Projectile.NewProjectile(Player.GetSource_FromThis(), Player.Center, vel, ProjectileID.MagicDagger, damage, 2f, Player.whoAmI);
                    }
                }
            }
        }
    }

    public static class WrathsCleaverPlayerExtensions
    {
        public static WrathsCleaverPlayer WrathsCleaver(this Player player)
            => player.GetModPlayer<WrathsCleaverPlayer>();
    }
}
