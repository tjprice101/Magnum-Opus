using Terraria;
using Terraria.ModLoader;
using MagnumOpus.Common.Prefixes;

namespace MagnumOpus.Content.Common.Accessories
{
    /// <summary>
    /// Shared ModPlayer for the Melodic Attunement accessory system (§12 class-specific standalone accessories).
    /// Handles: Resonant Burn damage bonus, crit damage on burn enemies,
    /// hit-count healing per class, and universal crit damage bonus.
    /// Each equipped accessory adds to these values during UpdateAccessory.
    /// </summary>
    public class MelodicAttunementPlayer : ModPlayer
    {
        // Accumulated bonuses (reset each frame, set by accessories)
        public float resonantBurnDmgBonus;
        public float critDmgBonusOnBurn;
        public float critDmgAll;

        // Per-class attunement flags
        public bool meleeAttunement;
        public bool rangedAttunement;
        public bool magicAttunement;
        public bool summonAttunement;

        // Hit counters (persist until healed)
        private int meleeHitCount;
        private int rangedHitCount;
        private int magicHitCount;
        private int summonHitCount;

        private const int MeleeHealThreshold = 10;
        private const int RangedHealThreshold = 25;
        private const int MagicHealThreshold = 15;
        private const int SummonHealThreshold = 30;
        private const float HealPercent = 0.10f;

        public override void ResetEffects()
        {
            resonantBurnDmgBonus = 0f;
            critDmgBonusOnBurn = 0f;
            critDmgAll = 0f;
            meleeAttunement = false;
            rangedAttunement = false;
            magicAttunement = false;
            summonAttunement = false;
        }

        public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
        {
            bool hasBurn = target.HasBuff(ModContent.BuffType<ResonantBurnDebuff>());

            if (hasBurn && resonantBurnDmgBonus > 0f)
                modifiers.FinalDamage += resonantBurnDmgBonus;

            if (hasBurn && critDmgBonusOnBurn > 0f)
                modifiers.CritDamage += critDmgBonusOnBurn;

            if (critDmgAll > 0f)
                modifiers.CritDamage += critDmgAll;
        }

        public override void OnHitNPCWithItem(Item item, NPC target, NPC.HitInfo hit, int damageDone)
        {
            TryCountHit(target, item.DamageType);
        }

        public override void OnHitNPCWithProj(Projectile proj, NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (proj.owner == Player.whoAmI)
                TryCountHit(target, proj.DamageType);
        }

        private void TryCountHit(NPC target, DamageClass damageClass)
        {
            if (!target.HasBuff(ModContent.BuffType<ResonantBurnDebuff>()))
                return;

            if (meleeAttunement && damageClass.CountsAsClass(DamageClass.Melee))
            {
                meleeHitCount++;
                if (meleeHitCount >= MeleeHealThreshold)
                {
                    meleeHitCount = 0;
                    Player.Heal((int)(Player.statLifeMax2 * HealPercent));
                }
            }

            if (rangedAttunement && damageClass.CountsAsClass(DamageClass.Ranged))
            {
                rangedHitCount++;
                if (rangedHitCount >= RangedHealThreshold)
                {
                    rangedHitCount = 0;
                    Player.Heal((int)(Player.statLifeMax2 * HealPercent));
                }
            }

            if (magicAttunement && damageClass.CountsAsClass(DamageClass.Magic))
            {
                magicHitCount++;
                if (magicHitCount >= MagicHealThreshold)
                {
                    magicHitCount = 0;
                    Player.Heal((int)(Player.statLifeMax2 * HealPercent));
                }
            }

            if (summonAttunement && damageClass.CountsAsClass(DamageClass.Summon))
            {
                summonHitCount++;
                if (summonHitCount >= SummonHealThreshold)
                {
                    summonHitCount = 0;
                    Player.Heal((int)(Player.statLifeMax2 * HealPercent));
                }
            }
        }
    }
}
