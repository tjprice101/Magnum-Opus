using Terraria;
using Terraria.ModLoader;

namespace MagnumOpus.Content.Nachtmusik.Accessories
{
    /// <summary>Nocturne's Cadence: +12% melee damage, +5% melee crit.</summary>
    public class NocturnesCadenceBuff : ModBuff
    {
        public override string Texture => "MagnumOpus/Assets/BuffIcons/NocturnesCadenceBuff";
        public override void SetStaticDefaults() { Main.buffNoTimeDisplay[Type] = false; Main.debuff[Type] = false; }
        public override void Update(Player player, ref int buffIndex)
        {
            player.GetDamage(DamageClass.Melee) += 0.12f;
            player.GetCritChance(DamageClass.Melee) += 5;
        }
    }

    /// <summary>Rondo Allegro: +15% ranged damage, +10% ranged crit.</summary>
    public class RondoAllegroBuff : ModBuff
    {
        public override string Texture => "MagnumOpus/Assets/BuffIcons/RondoAllegroBuff";
        public override void SetStaticDefaults() { Main.buffNoTimeDisplay[Type] = false; Main.debuff[Type] = false; }
        public override void Update(Player player, ref int buffIndex)
        {
            player.GetDamage(DamageClass.Ranged) += 0.15f;
            player.GetCritChance(DamageClass.Ranged) += 10;
        }
    }

    /// <summary>Starlit Fervor: +25% summon damage (timed proc every 8s/6s at night).</summary>
    public class StarlitFervorBuff : ModBuff
    {
        public override string Texture => "MagnumOpus/Assets/BuffIcons/StarlitFervorBuff";
        public override void SetStaticDefaults() { Main.buffNoTimeDisplay[Type] = false; Main.debuff[Type] = false; }
        public override void Update(Player player, ref int buffIndex)
        {
            player.GetDamage(DamageClass.Summon) += 0.25f;
        }
    }

    /// <summary>Eine Kleine: +12% all damage, +5% crit.</summary>
    public class EineKleineBuff : ModBuff
    {
        public override string Texture => "MagnumOpus/Assets/BuffIcons/EineKleineBuff";
        public override void SetStaticDefaults() { Main.buffNoTimeDisplay[Type] = false; Main.debuff[Type] = false; }
        public override void Update(Player player, ref int buffIndex)
        {
            player.GetDamage(DamageClass.Generic) += 0.12f;
            player.GetCritChance(DamageClass.Generic) += 5;
        }
    }

    /// <summary>Lullaby of the Stars: wing airborne night buff. Dodge handled in ModPlayer.FreeDodge.</summary>
    public class LullabyOfTheStarsBuff : ModBuff
    {
        public override string Texture => "MagnumOpus/Assets/BuffIcons/LullabyOfTheStarsBuff";
        public override void SetStaticDefaults() { Main.buffNoTimeDisplay[Type] = false; Main.debuff[Type] = false; }
        public override void Update(Player player, ref int buffIndex) { }
    }

    /// <summary>Daydream Minuet: wing airborne day buff (weaker). Dodge handled in ModPlayer.FreeDodge.</summary>
    public class DaydreamMinuetBuff : ModBuff
    {
        public override string Texture => "MagnumOpus/Assets/BuffIcons/DaydreamMinuetBuff";
        public override void SetStaticDefaults() { Main.buffNoTimeDisplay[Type] = false; Main.debuff[Type] = false; }
        public override void Update(Player player, ref int buffIndex) { }
    }

    /// <summary>Notturno: melee chain buff, +8% melee damage, +5% melee crit (2s).</summary>
    public class NotturnoBuff : ModBuff
    {
        public override string Texture => "MagnumOpus/Assets/BuffIcons/NotturnoBuff";
        public override void SetStaticDefaults() { Main.buffNoTimeDisplay[Type] = false; Main.debuff[Type] = false; }
        public override void Update(Player player, ref int buffIndex)
        {
            player.GetDamage(DamageClass.Melee) += 0.08f;
            player.GetCritChance(DamageClass.Melee) += 5;
        }
    }

    /// <summary>Serenade's Refrain: mage chain buff, boosted mana regen (3s).</summary>
    public class SerenadeRefrainBuff : ModBuff
    {
        public override string Texture => "MagnumOpus/Assets/BuffIcons/SerenadeRefrainBuff";
        public override void SetStaticDefaults() { Main.buffNoTimeDisplay[Type] = false; Main.debuff[Type] = false; }
        public override void Update(Player player, ref int buffIndex)
        {
            player.manaRegenBonus += 15;
        }
    }

    /// <summary>Phantom Step: mobility chain buff, reduced enemy aggro.</summary>
    public class PhantomStepBuff : ModBuff
    {
        public override string Texture => "MagnumOpus/Assets/BuffIcons/PhantomStepBuff";
        public override void SetStaticDefaults() { Main.buffNoTimeDisplay[Type] = false; Main.debuff[Type] = false; }
        public override void Update(Player player, ref int buffIndex)
        {
            player.aggro -= 400;
        }
    }

    /// <summary>Nachtmusik Wing HP Amplification: 50% damage reduction (doubles effective HP) for 40 seconds.</summary>
    public class NachtmusikWingAmplifyBuff : ModBuff
    {
        public override string Texture => "MagnumOpus/Assets/BuffIcons/NachtmusikWingAmplifyBuff";
        public override void SetStaticDefaults() { Main.buffNoTimeDisplay[Type] = false; Main.debuff[Type] = false; }
        public override void Update(Player player, ref int buffIndex)
        {
            player.endurance += 0.50f; // 50% damage reduction = doubles effective HP
        }
    }
}
