using System;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;
using MagnumOpus.Common.Systems.UI;
using MagnumOpus.Common.Utilities;
using MagnumOpus.Content.EnigmaVariations.ResonantWeapons.TheUnresolvedCadence;

namespace MagnumOpus.Content.EnigmaVariations.ResonantWeapons.TheUnresolvedCadence.Utilities
{
    public sealed class CadencePlayer : ModPlayer, IResonantOverdrive
    {
        /// <summary>Current combo phase: 0=VoidCleave, 1=ParadoxSlash, 2=DimensionalSeverance.</summary>
        public int ComboPhase;

        /// <summary>Stacks toward Paradox Collapse (0–10). At 10 → triggers 3× damage ultimate.</summary>
        public int InevitabilityStacks;

        /// <summary>Visual buildup intensity (0–1), drives VFX brightness/scale.</summary>
        public float CadenceIntensity;

        /// <summary>Set true on the frame Paradox Collapse triggers (10 stacks consumed).</summary>
        public bool ParadoxCollapseThisFrame;

        // === Charge Meter (VERY slow — 2% per hit, ~50 hits to fill) ===
        public float Charge = 0f;
        public const float ChargePerHit = 0.02f;
        public const float MaxCharge = 1.0f;
        public bool IsHoldingUnresolvedCadence = false;
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
            ParadoxCollapseThisFrame = false;
            IsHoldingUnresolvedCadence = false;
        }

        public override void PostUpdate()
        {
            // CadenceIntensity decays naturally when not actively swinging
            if (CadenceIntensity > 0f && Player.itemAnimation <= 0)
            {
                CadenceIntensity = MathHelper.Clamp(CadenceIntensity - 0.015f, 0f, 1f);
            }
        }

        public override void OnHitNPCWithProj(Projectile proj, NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (proj.type == ModContent.ProjectileType<TheUnresolvedCadenceSwing>())
                AddCharge(target.life <= 0 ? 0.08f : ChargePerHit);
        }

        // === IResonantOverdrive ===
        bool IResonantOverdrive.IsHoldingOverdriveWeapon => IsHoldingUnresolvedCadence;
        float IResonantOverdrive.OverdriveCharge => Charge;
        bool IResonantOverdrive.IsOverdriveReady => IsChargeFull;
        Color IResonantOverdrive.OverdriveLowColor => new Color(35, 20, 55);
        Color IResonantOverdrive.OverdriveHighColor => new Color(140, 90, 205);

        bool IResonantOverdrive.ActivateOverdrive(Player player)
        {
            if (player.whoAmI != Main.myPlayer)
                return true;

            foreach (NPC npc in NpcTargetingUtils.EnumerateHostiles(player.Center, 700f))
            {
                if (npc.boss)
                    continue;
                int damage = Math.Max(1, (int)(npc.lifeMax * 0.02f));
                npc.SimpleStrikeNPC(damage, 0, false, 0f, DamageClass.Melee, false, 0f, true);
            }

            ConsumeCharge();
            return true;
        }
    }

    public static class CadencePlayerExtensions
    {
        public static CadencePlayer Cadence(this Player player) => player.GetModPlayer<CadencePlayer>();
    }
}
