using Terraria;
using Terraria.ModLoader;

namespace MagnumOpus.Content.DiesIrae.Accessories
{
    // === PLAYER BUFFS ===

    /// <summary>Tuba Mirum: Restores 8% max mana over 3s, +8% magic damage on magic kill.</summary>
    public class TubaMirum : ModBuff
    {
        public override string Texture => "MagnumOpus/Assets/BuffIcons/TubaMirum";
        public override void SetStaticDefaults()
        {
            Main.buffNoTimeDisplay[Type] = false;
            Main.debuff[Type] = false;
        }
        public override void Update(Player player, ref int buffIndex)
        {
            player.GetDamage(DamageClass.Magic) += 0.08f;
            player.manaRegenBonus += 20;
        }
    }

    /// <summary>Executioner's Focus: +15% ranged damage to bosses for 2s on ranged crit vs boss.</summary>
    public class ExecutionersFocus : ModBuff
    {
        public override string Texture => "MagnumOpus/Assets/BuffIcons/ExecutionersFocus";
        public override void SetStaticDefaults()
        {
            Main.buffNoTimeDisplay[Type] = false;
            Main.debuff[Type] = false;
        }
        public override void Update(Player player, ref int buffIndex)
        {
            player.GetDamage(DamageClass.Ranged) += 0.15f;
        }
    }

    /// <summary>Unstoppable Fury: Knockback immunity, 25% contact damage as fire thorns at 175+ momentum.</summary>
    public class UnstoppableFury : ModBuff
    {
        public override string Texture => "MagnumOpus/Assets/BuffIcons/UnstoppableFury";
        public override void SetStaticDefaults()
        {
            Main.buffNoTimeDisplay[Type] = false;
            Main.debuff[Type] = false;
        }
        public override void Update(Player player, ref int buffIndex)
        {
            player.noKnockback = true;
            player.thorns += 0.25f;
        }
    }

    // === ENEMY DEBUFFS ===

    /// <summary>Wrathfire: 4 fire DPS, -5 defense for 3s on melee crits.</summary>
    public class Wrathfire : ModBuff
    {
        public override string Texture => "MagnumOpus/Assets/BuffIcons/Wrathfire";
        public override void SetStaticDefaults()
        {
            Main.debuff[Type] = true;
            Main.pvpBuff[Type] = true;
            Main.buffNoSave[Type] = true;
        }
    }

    /// <summary>Lacrimosa: +10% increased magic damage taken for 4s (5% proc on magic hit).</summary>
    public class Lacrimosa : ModBuff
    {
        public override string Texture => "MagnumOpus/Assets/BuffIcons/Lacrimosa";
        public override void SetStaticDefaults()
        {
            Main.debuff[Type] = true;
            Main.pvpBuff[Type] = true;
            Main.buffNoSave[Type] = true;
        }
    }

    /// <summary>Confutatis: -10 def, +15% damage taken (Infernal Rampart shield break).</summary>
    public class Confutatis : ModBuff
    {
        public override string Texture => "MagnumOpus/Assets/BuffIcons/Confutatis";
        public override void SetStaticDefaults()
        {
            Main.debuff[Type] = true;
            Main.pvpBuff[Type] = true;
            Main.buffNoSave[Type] = true;
        }
    }

    /// <summary>Quantus Tremor: 8 fire DPS, -8% attack speed for 3s on attackers (Infernal Rampart).</summary>
    public class QuantusTremor : ModBuff
    {
        public override string Texture => "MagnumOpus/Assets/BuffIcons/QuantusTremor";
        public override void SetStaticDefaults()
        {
            Main.debuff[Type] = true;
            Main.pvpBuff[Type] = true;
            Main.buffNoSave[Type] = true;
        }
    }

    /// <summary>Mors Stupebit: Feared, -10 defense for 2s on nearby enemies (Infernal Rampart shield break).</summary>
    public class MorsStupebit : ModBuff
    {
        public override string Texture => "MagnumOpus/Assets/BuffIcons/MorsStupebit";
        public override void SetStaticDefaults()
        {
            Main.debuff[Type] = true;
            Main.pvpBuff[Type] = true;
            Main.buffNoSave[Type] = true;
        }
    }

    /// <summary>Condemned: +15% minion DMG taken, -5 def (Infernal Executioner's Sight summoner debuff).</summary>
    public class Condemned : ModBuff
    {
        public override string Texture => "MagnumOpus/Assets/BuffIcons/Condemned";
        public override void SetStaticDefaults()
        {
            Main.debuff[Type] = true;
            Main.pvpBuff[Type] = true;
            Main.buffNoSave[Type] = true;
        }
    }

    /// <summary>Chains of Requiem: -25% speed, +15% DMG taken, no regen (Infernal Rampart).</summary>
    public class ChainsOfRequiem : ModBuff
    {
        public override string Texture => "MagnumOpus/Assets/BuffIcons/ChainsOfRequiem";
        public override void SetStaticDefaults()
        {
            Main.debuff[Type] = true;
            Main.pvpBuff[Type] = true;
            Main.buffNoSave[Type] = true;
        }
    }

    /// <summary>Infernal Choir: +15% increased minion damage taken for 3s on minion crits.</summary>
    public class InfernalChoir : ModBuff
    {
        public override string Texture => "MagnumOpus/Assets/BuffIcons/InfernalChoir";
        public override void SetStaticDefaults()
        {
            Main.debuff[Type] = true;
            Main.pvpBuff[Type] = true;
            Main.buffNoSave[Type] = true;
        }
    }

    /// <summary>Scorched Earth: On Fire!, -10% movement for 2s on enemies passed at max momentum.</summary>
    public class ScorchedEarth : ModBuff
    {
        public override string Texture => "MagnumOpus/Assets/BuffIcons/ScorchedEarth";
        public override void SetStaticDefaults()
        {
            Main.debuff[Type] = true;
            Main.pvpBuff[Type] = true;
            Main.buffNoSave[Type] = true;
        }
    }
}
