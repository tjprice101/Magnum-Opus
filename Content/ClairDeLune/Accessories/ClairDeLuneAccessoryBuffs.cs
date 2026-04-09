using Terraria;
using Terraria.ModLoader;

namespace MagnumOpus.Content.ClairDeLune.Accessories
{
    /// <summary>Reverie: +15% melee damage, +5% dodge (FreeDodge).</summary>
    public class ReverieBuff : ModBuff
    {
        public override string Texture => "MagnumOpus/Assets/BuffIcons/ReverieBuff";
        public override void SetStaticDefaults() { Main.buffNoTimeDisplay[Type] = false; Main.debuff[Type] = false; }
        public override void Update(Player player, ref int buffIndex)
        {
            player.GetDamage(DamageClass.Melee) += 0.15f;
        }
    }

    /// <summary>Arabesque: +12% magic damage, -15% mana cost.</summary>
    public class ArabesqueBuff : ModBuff
    {
        public override string Texture => "MagnumOpus/Assets/BuffIcons/ArabesqueBuff";
        public override void SetStaticDefaults() { Main.buffNoTimeDisplay[Type] = false; Main.debuff[Type] = false; }
        public override void Update(Player player, ref int buffIndex)
        {
            player.GetDamage(DamageClass.Magic) += 0.12f;
            player.manaCost -= 0.15f;
        }
    }

    /// <summary>Reflets dans l'Eau: +12% ranged damage, +8% ranged crit.</summary>
    public class RefletsDansLEauBuff : ModBuff
    {
        public override string Texture => "MagnumOpus/Assets/BuffIcons/RefletsDansLEauBuff";
        public override void SetStaticDefaults() { Main.buffNoTimeDisplay[Type] = false; Main.debuff[Type] = false; }
        public override void Update(Player player, ref int buffIndex)
        {
            player.GetDamage(DamageClass.Ranged) += 0.12f;
            player.GetCritChance(DamageClass.Ranged) += 8;
        }
    }

    /// <summary>Clair: +10% summon damage, +3 life regen. Night: +5% dodge, 5s.</summary>
    public class ClairBuff : ModBuff
    {
        public override string Texture => "MagnumOpus/Assets/BuffIcons/ClairBuff";
        public override void SetStaticDefaults() { Main.buffNoTimeDisplay[Type] = false; Main.debuff[Type] = false; }
        public override void Update(Player player, ref int buffIndex)
        {
            player.GetDamage(DamageClass.Summon) += 0.10f;
            player.lifeRegen += 3;
        }
    }

    /// <summary>Prelude: +5% dodge, enemies have 10% miss chance, +2 life regen (Wings of Moonlit Reverie flying).</summary>
    public class PreludeBuff : ModBuff
    {
        public override string Texture => "MagnumOpus/Assets/BuffIcons/PreludeBuff";
        public override void SetStaticDefaults() { Main.buffNoTimeDisplay[Type] = false; Main.debuff[Type] = false; }
        public override void Update(Player player, ref int buffIndex)
        {
            player.lifeRegen += 2;
        }
    }

    /// <summary>Clair de Lune: +10% all damage, +10% dodge (Reverie Drift dash proc). Legacy — kept for backward compat.</summary>
    public class ClairDeLuneBuff : ModBuff
    {
        public override string Texture => "MagnumOpus/Assets/BuffIcons/ClairDeLuneBuff";
        public override void SetStaticDefaults() { Main.buffNoTimeDisplay[Type] = false; Main.debuff[Type] = false; }
        public override void Update(Player player, ref int buffIndex)
        {
            player.GetDamage(DamageClass.Generic) += 0.10f;
        }
    }

    /// <summary>Clair de Lune Wing HP Amplification: 50% damage reduction (doubles effective HP) for 55 seconds.</summary>
    public class ClairDeLuneWingAmplifyBuff : ModBuff
    {
        public override string Texture => "MagnumOpus/Assets/BuffIcons/ClairDeLuneWingAmplifyBuff";
        public override void SetStaticDefaults() { Main.buffNoTimeDisplay[Type] = false; Main.debuff[Type] = false; }
        public override void Update(Player player, ref int buffIndex)
        {
            player.endurance += 0.50f; // 50% damage reduction = doubles effective HP
        }
    }
}