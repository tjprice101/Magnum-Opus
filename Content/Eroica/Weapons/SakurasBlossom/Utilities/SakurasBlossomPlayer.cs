using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;

namespace MagnumOpus.Content.Eroica.Weapons.SakurasBlossom.Utilities
{
    public class SakurasBlossomPlayer : ModPlayer
    {
        public float Charge = 0f;
        public const float ChargePerSwingHit = 0.08f;
        public const float ChargePerKill = 0.12f;
        public const float MaxCharge = 1.0f;
        public bool IsHoldingSakura = false;
        public bool IsChargeFull => Charge >= MaxCharge;

        public void AddCharge(float amount)
        {
            Charge = MathHelper.Clamp(Charge + amount, 0f, MaxCharge);
        }

        public void ConsumeCharge()
        {
            Charge = 0f;
        }

        public override void ResetEffects()
        {
            IsHoldingSakura = false;
        }

        public override void OnHitNPCWithProj(Projectile proj, NPC target, NPC.HitInfo hit, int damageDone)
        {
            // Only the swing projectile itself counts as true melee
            if (proj.type != ModContent.ProjectileType<SakurasBlossomSwing>()) return;

            float charge = target.life <= 0 ? ChargePerKill : ChargePerSwingHit;
            AddCharge(charge);
        }
    }

    public static class SakurasBlossomPlayerExtensions
    {
        public static SakurasBlossomPlayer SakurasBlossom(this Player player)
            => player.GetModPlayer<SakurasBlossomPlayer>();
    }
}
