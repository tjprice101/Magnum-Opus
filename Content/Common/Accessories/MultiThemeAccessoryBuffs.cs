using Terraria;
using Terraria.ModLoader;

namespace MagnumOpus.Content.Common.Accessories
{
    // =============================================
    // TWO-THEME COMBINATION BUFFS
    // =============================================

    /// <summary>Azure Immolation: Moonlight + La Campanella — blue fire DoT for 4s, 8% magic damage/s. Night: doubled + spreads on kill.</summary>
    public class AzureImmolation : ModBuff
    {
        public override string Texture => "MagnumOpus/Assets/BuffIcons/AzureImmolation";
        public override void SetStaticDefaults()
        {
            Main.debuff[Type] = true;
            Main.pvpBuff[Type] = true;
            Main.buffNoSave[Type] = true;
        }
    }

    /// <summary>Burning Pas de Deux: La Campanella + Swan Lake — proximity debuff, 3% weapon damage/s, -10% move speed.</summary>
    public class BurningPasDeDeux : ModBuff
    {
        public override string Texture => "MagnumOpus/Assets/BuffIcons/BurningPasDeDeux";
        public override void SetStaticDefaults()
        {
            Main.debuff[Type] = true;
            Main.pvpBuff[Type] = true;
            Main.buffNoSave[Type] = true;
        }
    }

    /// <summary>Dance of Death: Eroica + Enigma — on kill for 6s: ignore 20% defense, every 3rd hit deals +50% damage.</summary>
    public class DanceOfDeath : ModBuff
    {
        public override string Texture => "MagnumOpus/Assets/BuffIcons/DanceOfDeath";
        public override void SetStaticDefaults()
        {
            Main.buffNoTimeDisplay[Type] = false;
            Main.debuff[Type] = false;
        }
        public override void Update(Player player, ref int buffIndex)
        {
            player.GetArmorPenetration(DamageClass.Generic) += 20;
        }
    }

    /// <summary>Moonlit Reverie: Moonlight + Swan Lake — stacking per second (max 10), +1% dodge/stack. At 10: Silver Cascade.</summary>
    public class MoonlitReverie : ModBuff
    {
        public override string Texture => "MagnumOpus/Assets/BuffIcons/MoonlitReverie";
        public override void SetStaticDefaults()
        {
            Main.buffNoTimeDisplay[Type] = false;
            Main.debuff[Type] = false;
        }
    }

    /// <summary>Silver Cascade: Moonlight + Swan Lake — triggered at 10 Moonlit Reverie stacks: AoE damage echo.</summary>
    public class SilverCascade : ModBuff
    {
        public override string Texture => "MagnumOpus/Assets/BuffIcons/SilverCascade";
        public override void SetStaticDefaults()
        {
            Main.buffNoTimeDisplay[Type] = false;
            Main.debuff[Type] = false;
        }
    }

    /// <summary>Void Immolation: La Campanella + Enigma — 5% on hit for 5s: enemy takes 2% more damage (scales to 30% with hits).</summary>
    public class VoidImmolation : ModBuff
    {
        public override string Texture => "MagnumOpus/Assets/BuffIcons/VoidImmolation";
        public override void SetStaticDefaults()
        {
            Main.debuff[Type] = true;
            Main.pvpBuff[Type] = true;
            Main.buffNoSave[Type] = true;
        }
    }

    /// <summary>Requiem's Mark: La Campanella + Enigma + Swan Lake — enemies hit by Paradox + Tolling Death, +15% damage taken for 8s.</summary>
    public class RequiemsMark : ModBuff
    {
        public override string Texture => "MagnumOpus/Assets/BuffIcons/RequiemsMark";
        public override void SetStaticDefaults()
        {
            Main.debuff[Type] = true;
            Main.pvpBuff[Type] = true;
            Main.buffNoSave[Type] = true;
        }
    }

    /// <summary>Arabesque Ascension: Eroica + Swan Lake — while airborne for 10s: +30% attack speed, +15% reach, 0.5% max HP heal.</summary>
    public class ArabesqueAscension : ModBuff
    {
        public override string Texture => "MagnumOpus/Assets/BuffIcons/ArabesqueAscension";
        public override void SetStaticDefaults()
        {
            Main.buffNoTimeDisplay[Type] = false;
            Main.debuff[Type] = false;
        }
        public override void Update(Player player, ref int buffIndex)
        {
            player.GetAttackSpeed(DamageClass.Generic) += 0.30f;
        }
    }

    /// <summary>Valor of the Radiant Swan: Eroica + Swan Lake — while airborne for 10s: +30% attack speed, +15% reach, 0.5% HP heal.</summary>
    public class ValorOfTheRadiantSwan : ModBuff
    {
        public override string Texture => "MagnumOpus/Assets/BuffIcons/ValorOfTheRadiantSwan";
        public override void SetStaticDefaults()
        {
            Main.buffNoTimeDisplay[Type] = false;
            Main.debuff[Type] = false;
        }
        public override void Update(Player player, ref int buffIndex)
        {
            player.GetAttackSpeed(DamageClass.Generic) += 0.30f;
        }
    }

    // =============================================
    // THREE-THEME COMBINATION BUFFS
    // =============================================

    /// <summary>Nocturnal Trinity: Moonlight + La Campanella + Enigma — rotating buff cycling 8s: Moon/Bell/Void phases.</summary>
    public class NocturnalTrinity : ModBuff
    {
        public override string Texture => "MagnumOpus/Assets/BuffIcons/NocturnalTrinity";
        public override void SetStaticDefaults()
        {
            Main.buffNoTimeDisplay[Type] = false;
            Main.debuff[Type] = false;
        }
    }

    /// <summary>Radiant Crescendo: Eroica + Moonlight + Swan Lake — consecutive hit stacks (max 20): tiers at 5/10/15/20.</summary>
    public class RadiantCrescendo : ModBuff
    {
        public override string Texture => "MagnumOpus/Assets/BuffIcons/RadiantCrescendo";
        public override void SetStaticDefaults()
        {
            Main.buffNoTimeDisplay[Type] = false;
            Main.debuff[Type] = false;
        }
    }

    /// <summary>Fortissimo: Eroica + Moonlight + Swan Lake — at 20 RadiantCrescendo stacks: next hit deals 5x damage.</summary>
    public class Fortissimo : ModBuff
    {
        public override string Texture => "MagnumOpus/Assets/BuffIcons/Fortissimo";
        public override void SetStaticDefaults()
        {
            Main.buffNoTimeDisplay[Type] = false;
            Main.debuff[Type] = false;
        }
    }

    // =============================================
    // ALL-THEME (COMPLETE HARMONY) BUFFS
    // =============================================

    /// <summary>Harmonic Resonance: All 5 themes stacking system (max 5). 1=+5% DR, 3=+10% speed/+5 regen, 5=Full Harmony.</summary>
    public class HarmonicResonance : ModBuff
    {
        public override string Texture => "MagnumOpus/Assets/BuffIcons/HarmonicResonance";
        public override void SetStaticDefaults()
        {
            Main.buffNoTimeDisplay[Type] = false;
            Main.debuff[Type] = false;
        }
    }

    /// <summary>Full Harmony: Triggered at 5 Harmonic Resonance stacks for 8s — +25% all damage, +15% dodge, 0.5% HP heal/hit.</summary>
    public class FullHarmony : ModBuff
    {
        public override string Texture => "MagnumOpus/Assets/BuffIcons/FullHarmony";
        public override void SetStaticDefaults()
        {
            Main.buffNoTimeDisplay[Type] = false;
            Main.debuff[Type] = false;
        }
        public override void Update(Player player, ref int buffIndex)
        {
            player.GetDamage(DamageClass.Generic) += 0.25f;
        }
    }

    /// <summary>Dissonance: Enemies w/ 3+ different theme debuffs for 8s — take 20% more damage from all sources.</summary>
    public class Dissonance : ModBuff
    {
        public override string Texture => "MagnumOpus/Assets/BuffIcons/Dissonance";
        public override void SetStaticDefaults()
        {
            Main.debuff[Type] = true;
            Main.pvpBuff[Type] = true;
            Main.buffNoSave[Type] = true;
        }
    }

    // =============================================
    // CODA ULTIMATE BUFF
    // =============================================

    /// <summary>Coda Resonance: All 10 themes — +35% all damage, +20% dodge, 1% HP heal/hit for 10s, all procs doubled.</summary>
    public class CodaResonance : ModBuff
    {
        public override string Texture => "MagnumOpus/Assets/BuffIcons/CodaResonance";
        public override void SetStaticDefaults()
        {
            Main.buffNoTimeDisplay[Type] = false;
            Main.debuff[Type] = false;
        }
        public override void Update(Player player, ref int buffIndex)
        {
            player.GetDamage(DamageClass.Generic) += 0.35f;
        }
    }

    // =============================================
    // REQUIEM'S JUDGMENT (Unique ultimate trigger)
    // =============================================

    /// <summary>Requiem's Judgment: Ultimate judgment effect from complete harmony system.</summary>
    public class RequiemsJudgment : ModBuff
    {
        public override string Texture => "MagnumOpus/Assets/BuffIcons/RequiemsJudgment";
        public override void SetStaticDefaults()
        {
            Main.debuff[Type] = true;
            Main.pvpBuff[Type] = true;
            Main.buffNoSave[Type] = true;
        }
    }
}
