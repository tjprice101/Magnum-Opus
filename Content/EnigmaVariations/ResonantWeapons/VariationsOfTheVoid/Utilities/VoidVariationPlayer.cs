using System;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;
using MagnumOpus.Common.Systems.UI;
using MagnumOpus.Common.Utilities;
using MagnumOpus.Content.EnigmaVariations.ResonantWeapons.VariationsOfTheVoid;

namespace MagnumOpus.Content.EnigmaVariations.ResonantWeapons.VariationsOfTheVoid.Utilities
{
    public sealed class VoidVariationPlayer : ModPlayer, IResonantOverdrive
    {
        /// <summary>Current combo phase: 0=VoidWhisper, 1=AbyssalEcho, 2=RiftSunderFinisher.</summary>
        public int VoidComboPhase;

        /// <summary>Resonance stacks accumulated across swings, powering the finisher tri-beam.</summary>
        public int VariationStack;

        /// <summary>Visual buildup intensity (0–1), drives VFX brightness/scale.</summary>
        public float VoidIntensity;

        /// <summary>Set true on the frame the tri-beam convergence fires.</summary>
        public bool ConvergenceThisFrame;

        // === Charge Meter ===
        public float Charge = 0f;
        public const float ChargePerHit = 0.05f;
        public const float MaxCharge = 1.0f;
        public bool IsHoldingVariationsOfTheVoid = false;
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
            ConvergenceThisFrame = false;
            IsHoldingVariationsOfTheVoid = false;
        }

        public override void PostUpdate()
        {
            // VoidIntensity decays naturally when not actively swinging
            if (VoidIntensity > 0f && Player.itemAnimation <= 0)
            {
                VoidIntensity = MathHelper.Clamp(VoidIntensity - 0.015f, 0f, 1f);
            }
        }

        public override void OnHitNPCWithProj(Projectile proj, NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (proj.type == ModContent.ProjectileType<VariationsOfTheVoidSwing>())
                AddCharge(target.life <= 0 ? 0.15f : ChargePerHit);
        }

        // === IResonantOverdrive ===
        bool IResonantOverdrive.IsHoldingOverdriveWeapon => IsHoldingVariationsOfTheVoid;
        float IResonantOverdrive.OverdriveCharge => Charge;
        bool IResonantOverdrive.IsOverdriveReady => IsChargeFull;
        Color IResonantOverdrive.OverdriveLowColor => new Color(70, 20, 110);
        Color IResonantOverdrive.OverdriveHighColor => new Color(175, 90, 230);

        bool IResonantOverdrive.ActivateOverdrive(Player player)
        {
            if (player.whoAmI != Main.myPlayer)
                return true;

            int baseDamage = Math.Max(1, player.HeldItem.damage);
            foreach (NPC npc in NpcTargetingUtils.EnumerateHostiles(player.Center, 300f))
            {
                Vector2 dir = (npc.Center - player.Center).SafeNormalize(Vector2.UnitX * player.direction);
                if (Vector2.Dot(dir, Vector2.UnitX * player.direction) < -0.35f)
                    continue;

                int damage = (int)(baseDamage * 3f);
                npc.SimpleStrikeNPC(damage, player.direction, false, 0f, DamageClass.Melee, false, 0f, true);

                foreach (NPC secondary in NpcTargetingUtils.EnumerateHostiles(npc.Center, 110f))
                    secondary.SimpleStrikeNPC((int)(baseDamage * 1.5f), 0, false, 0f, DamageClass.Melee, false, 0f, true);
            }

            ConsumeCharge();
            return true;
        }
    }

    public static class VoidVariationPlayerExtensions
    {
        public static VoidVariationPlayer VoidVariation(this Player player) => player.GetModPlayer<VoidVariationPlayer>();
    }
}
