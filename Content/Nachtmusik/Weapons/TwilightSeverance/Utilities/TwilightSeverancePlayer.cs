using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;
using MagnumOpus.Common.Systems.UI;
using MagnumOpus.Common.Utilities;

namespace MagnumOpus.Content.Nachtmusik.Weapons.TwilightSeverance.Utilities
{
    public class TwilightSeverancePlayer : ModPlayer, IResonantOverdrive
    {
        // === Charge Meter ===
        public float Charge = 0f;
        public const float ChargePerHit = 0.05f;
        public const float MaxCharge = 1.0f;
        public bool IsHoldingTwilightSeverance = false;
        public bool IsChargeFull => Charge >= MaxCharge;

        // Twilight Severance special state: lines to enemies
        public bool LinesActive = false;

        // === Mark/Execute System ===
        private readonly List<int> _markedNpcs = new();
        private int _markTimer;

        public IReadOnlyList<int> MarkedNpcs => _markedNpcs;
        public bool HasMarkedTargets => _markedNpcs.Count > 0;

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
            IsHoldingTwilightSeverance = false;
        }

        public override void PostUpdate()
        {
            if (_markTimer > 0)
                _markTimer--;
            else if (_markedNpcs.Count > 0)
            {
                _markedNpcs.Clear();
                LinesActive = false;
            }
        }

        public override void OnHitNPCWithProj(Projectile proj, NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (proj.type == ModContent.ProjectileType<Projectiles.TwilightSeveranceSwing>())
                AddCharge(target.life <= 0 ? 0.15f : ChargePerHit);
        }

        // === IResonantOverdrive ===
        bool IResonantOverdrive.IsHoldingOverdriveWeapon => IsHoldingTwilightSeverance;
        float IResonantOverdrive.OverdriveCharge => Charge;
        bool IResonantOverdrive.IsOverdriveReady => IsChargeFull;
        Color IResonantOverdrive.OverdriveLowColor => new Color(65, 80, 160);
        Color IResonantOverdrive.OverdriveHighColor => new Color(160, 200, 255);

        bool IResonantOverdrive.ActivateOverdrive(Player player)
        {
            if (player.whoAmI != Main.myPlayer)
                return true;

            int baseDamage = Math.Max(1, player.HeldItem.damage);

            if (_markedNpcs.Count > 0)
            {
                // Execute phase: kill marked non-bosses, heavy damage to bosses
                for (int i = _markedNpcs.Count - 1; i >= 0; i--)
                {
                    int who = _markedNpcs[i];
                    if (who < 0 || who >= Main.maxNPCs)
                        continue;

                    NPC npc = Main.npc[who];
                    if (!npc.active || npc.friendly || npc.dontTakeDamage)
                        continue;

                    if (!npc.boss)
                        npc.SimpleStrikeNPC(npc.life + 1, 0, true, 0f, DamageClass.Melee, false, 0f, true);
                    else
                        npc.SimpleStrikeNPC(baseDamage * 8, 0, false, 0f, DamageClass.Melee, false, 0f, true);
                }

                _markedNpcs.Clear();
                _markTimer = 0;
                LinesActive = false;
            }
            else
            {
                // Mark phase: mark enemies in range
                _markedNpcs.Clear();
                foreach (NPC npc in NpcTargetingUtils.EnumerateHostiles(player.Center, 900f))
                {
                    _markedNpcs.Add(npc.whoAmI);
                    if (_markedNpcs.Count >= 80)
                        break;
                }

                _markTimer = 300;
                LinesActive = true;
            }

            ConsumeCharge();
            return true;
        }
    }

    public static class TwilightSeverancePlayerExtensions
    {
        public static TwilightSeverancePlayer TwilightSeverance(this Player player)
            => player.GetModPlayer<TwilightSeverancePlayer>();
    }
}
