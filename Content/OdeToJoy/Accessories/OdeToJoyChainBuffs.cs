using Terraria;
using Terraria.ModLoader;

namespace MagnumOpus.Content.OdeToJoy.Accessories
{
    // === PLAYER BUFFS ===

    /// <summary>Jubilant Stride: +5% all damage, +3% dodge chance at max momentum.</summary>
    public class JubilantStride : ModBuff
    {
        public override string Texture => "MagnumOpus/Assets/BuffIcons/JubilantStride";
        public override void SetStaticDefaults()
        {
            Main.buffNoTimeDisplay[Type] = false;
            Main.debuff[Type] = false;
        }
        public override void Update(Player player, ref int buffIndex)
        {
            player.GetDamage(DamageClass.Generic) += 0.05f;
        }
    }

    /// <summary>Hymn of Fortitude: +8 defense, +5 life regen, +5% DR for 6s (standing still 2s).</summary>
    public class HymnOfFortitude : ModBuff
    {
        public override string Texture => "MagnumOpus/Assets/BuffIcons/HymnOfFortitude";
        public override void SetStaticDefaults()
        {
            Main.buffNoTimeDisplay[Type] = false;
            Main.debuff[Type] = false;
        }
        public override void Update(Player player, ref int buffIndex)
        {
            player.statDefense += 8;
            player.lifeRegen += 5;
            player.endurance += 0.05f;
        }
    }

    /// <summary>Triumphant Crescendo: +12% melee dmg, +8% crit, heals 8% max HP every 10th kill.</summary>
    public class TriumphantCrescendo : ModBuff
    {
        public override string Texture => "MagnumOpus/Assets/BuffIcons/TriumphantCrescendo";
        public override void SetStaticDefaults()
        {
            Main.buffNoTimeDisplay[Type] = false;
            Main.debuff[Type] = false;
        }
        public override void Update(Player player, ref int buffIndex)
        {
            player.GetDamage(DamageClass.Melee) += 0.12f;
            player.GetCritChance(DamageClass.Melee) += 8;
        }
    }

    /// <summary>Tutti Fortissimo: All minions +50% damage for 3s every 15s.</summary>
    public class TuttiFortissimo : ModBuff
    {
        public override string Texture => "MagnumOpus/Assets/BuffIcons/TuttiFortissimo";
        public override void SetStaticDefaults()
        {
            Main.buffNoTimeDisplay[Type] = false;
            Main.debuff[Type] = false;
        }
        public override void Update(Player player, ref int buffIndex)
        {
            player.GetDamage(DamageClass.Summon) += 0.50f;
        }
    }

    /// <summary>Arcane Jubilee: Restores 10 mana, +5% magic damage for 3s (20% on magic kill).</summary>
    public class ArcaneJubilee : ModBuff
    {
        public override string Texture => "MagnumOpus/Assets/BuffIcons/ArcaneJubilee";
        public override void SetStaticDefaults()
        {
            Main.buffNoTimeDisplay[Type] = false;
            Main.debuff[Type] = false;
        }
        public override void Update(Player player, ref int buffIndex)
        {
            player.GetDamage(DamageClass.Magic) += 0.05f;
            player.manaRegenBonus += 10;
        }
    }

    /// <summary>Hunter's Jubilation: +8% ranged crit, +5% ranged damage for 3s on ranged crit kills.</summary>
    public class HuntersJubilation : ModBuff
    {
        public override string Texture => "MagnumOpus/Assets/BuffIcons/HuntersJubilation";
        public override void SetStaticDefaults()
        {
            Main.buffNoTimeDisplay[Type] = false;
            Main.debuff[Type] = false;
        }
        public override void Update(Player player, ref int buffIndex)
        {
            player.GetDamage(DamageClass.Ranged) += 0.05f;
            player.GetCritChance(DamageClass.Ranged) += 8;
        }
    }

    // === ENEMY DEBUFFS ===

    /// <summary>Hymnal Anchor: Slowed enemy + defense reduction.</summary>
    public class HymnalAnchor : ModBuff
    {
        public override string Texture => "MagnumOpus/Assets/BuffIcons/HymnalAnchor";
        public override void SetStaticDefaults()
        {
            Main.debuff[Type] = true;
            Main.pvpBuff[Type] = true;
            Main.buffNoSave[Type] = true;
        }
    }
}
