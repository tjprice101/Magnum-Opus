using Microsoft.Xna.Framework;
using System.Collections.Generic;
using MagnumOpus.Content.MoonlightSonata.Weapons.IncisorOfMoonlight.Projectiles;
using Terraria;
using Terraria.ModLoader;

namespace MagnumOpus.Content.MoonlightSonata.Weapons.IncisorOfMoonlight.Utilities
{
    /// <summary>
    /// Per-player state tracker for the Incisor of Moonlight.
    /// Manages input listeners, staccato hit tracking, and lunar charge meter.
    /// </summary>
    public class IncisorPlayer : ModPlayer
    {
        public bool LungingDown = false;
        public bool rightClickListener = false;
        public bool mouseWorldListener = false;
        public Vector2 mouseWorld => Main.MouseWorld;

        // Lunar Charge Meter
        public float LunarCharge = 0f;
        public const float ChargePerHit = 0.06f;
        public const float ChargePerKill = 0.20f;
        public const float MaxCharge = 1.0f;
        public bool IsHoldingIncisor = false;
        public bool IsChargeFull => LunarCharge >= MaxCharge;

        private static HashSet<int> _incisorProjectileTypes;

        public void AddCharge(float amount)
        {
            LunarCharge = MathHelper.Clamp(LunarCharge + amount, 0f, MaxCharge);
        }

        public void ConsumeCharge()
        {
            LunarCharge = 0f;
        }

        /// <summary>
        /// Tracks staccato note hits per NPC for detonation mechanics.
        /// Key: NPC whoAmI, Value: hit count
        /// </summary>
        private Dictionary<int, int> staccatoHits = new Dictionary<int, int>();

        public override void ResetEffects()
        {
            LungingDown = false;
            rightClickListener = false;
            mouseWorldListener = false;
            IsHoldingIncisor = false;
        }

        public override void PostUpdate()
        {
            if (!LungingDown)
                return;
            Player.fullRotation = 0f;
        }

        private static HashSet<int> GetIncisorProjectileTypes()
        {
            if (_incisorProjectileTypes == null)
            {
                _incisorProjectileTypes = new HashSet<int>
                {
                    ModContent.ProjectileType<IncisorSwingProj>(),
                    ModContent.ProjectileType<LunarBeamProj>(),
                    ModContent.ProjectileType<SuperLunarOrbProj>(),
                    ModContent.ProjectileType<LunarZoneProj>(),
                    ModContent.ProjectileType<ConstellationSlash>(),
                    ModContent.ProjectileType<ConstellationSlashCreator>(),
                    ModContent.ProjectileType<CrescentMoonProj>(),
                    ModContent.ProjectileType<CrescentWaveProj>(),
                    ModContent.ProjectileType<LunarNova>(),
                    ModContent.ProjectileType<OrbitingNoteProj>(),
                    ModContent.ProjectileType<StaccatoNoteProj>(),
                };
            }
            return _incisorProjectileTypes;
        }

        public override void OnHitNPCWithProj(Projectile proj, NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (!GetIncisorProjectileTypes().Contains(proj.type)) return;

            float charge = ChargePerHit;
            if (target.life <= 0)
                charge = ChargePerKill;

            AddCharge(charge);
        }

        public override void Unload()
        {
            _incisorProjectileTypes = null;
        }

        /// <summary>
        /// Registers a staccato note hit on the specified NPC.
        /// </summary>
        public void RegisterStaccatoHit(int npcIndex)
        {
            if (staccatoHits.ContainsKey(npcIndex))
                staccatoHits[npcIndex]++;
            else
                staccatoHits[npcIndex] = 1;
        }

        /// <summary>
        /// Gets the number of staccato hits on the specified NPC.
        /// </summary>
        public int GetStaccatoHits(int npcIndex)
        {
            return staccatoHits.ContainsKey(npcIndex) ? staccatoHits[npcIndex] : 0;
        }

        /// <summary>
        /// Resets the staccato hit counter for the specified NPC after detonation.
        /// </summary>
        public void ResetStaccatoHits(int npcIndex)
        {
            if (staccatoHits.ContainsKey(npcIndex))
                staccatoHits[npcIndex] = 0;
        }
    }

    public static class IncisorPlayerExtensions
    {
        public static IncisorPlayer Incisor(this Player player) => player.GetModPlayer<IncisorPlayer>();
    }
}
