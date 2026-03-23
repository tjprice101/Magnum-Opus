using System.Collections.Generic;
using MagnumOpus.Content.Eroica.Weapons.CelestialValor.Projectiles;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;

namespace MagnumOpus.Content.Eroica.Weapons.CelestialValor.Utilities
{
    public class CelestialValorPlayer : ModPlayer
    {
        public float Charge = 0f;
        public const float ChargePerHit = 0.05f;
        public const float ChargePerKill = 0.15f;
        public const float MaxCharge = 1.0f;
        public bool IsHoldingCelestialValor = false;
        public bool IsChargeFull => Charge >= MaxCharge;

        /// <summary>Whether a flying blade projectile is currently active.</summary>
        public bool HasActiveFlyingBlade = false;

        /// <summary>Signal from the item's second right-click to tell the flying blade to ignite and hurl.</summary>
        public bool TriggerBladeHurl = false;

        private static HashSet<int> _valorProjectileTypes;

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
            IsHoldingCelestialValor = false;
        }

        private static HashSet<int> GetValorProjectileTypes()
        {
            if (_valorProjectileTypes == null)
            {
                _valorProjectileTypes = new HashSet<int>
                {
                    ModContent.ProjectileType<CelestialValorSwing>(),
                    ModContent.ProjectileType<ValorBeam>(),
                    ModContent.ProjectileType<ValorSlash>(),
                    ModContent.ProjectileType<ValorBoom>(),
                };
            }
            return _valorProjectileTypes;
        }

        public override void OnHitNPCWithProj(Projectile proj, NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (!GetValorProjectileTypes().Contains(proj.type)) return;

            float charge = ChargePerHit;
            if (target.life <= 0)
                charge = ChargePerKill;

            AddCharge(charge);
        }

        public override void Unload()
        {
            _valorProjectileTypes = null;
        }
    }

    public static class CelestialValorPlayerExtensions
    {
        public static CelestialValorPlayer CelestialValor(this Player player)
            => player.GetModPlayer<CelestialValorPlayer>();
    }
}
