using Terraria;
using Terraria.ModLoader;

namespace MagnumOpus.Content.Fate.Accessories
{
    /// <summary>Astral Resonance: Magic crits grant for 3s — +8% magic damage, +5% magic crit.</summary>
    public class AstralResonance : ModBuff
    {
        public override string Texture => "MagnumOpus/Assets/BuffIcons/AstralResonance";
        public override void SetStaticDefaults()
        {
            Main.buffNoTimeDisplay[Type] = false;
            Main.debuff[Type] = false;
        }
        public override void Update(Player player, ref int buffIndex)
        {
            player.GetDamage(DamageClass.Magic) += 0.08f;
            player.GetCritChance(DamageClass.Magic) += 5;
        }
    }

    /// <summary>Constellation Mark: Applied by ranged crits for 3s — enemy takes +10% ranged damage.</summary>
    public class ConstellationMark : ModBuff
    {
        public override string Texture => "MagnumOpus/Assets/BuffIcons/ConstellationMark";
        public override void SetStaticDefaults()
        {
            Main.debuff[Type] = true;
            Main.pvpBuff[Type] = true;
            Main.buffNoSave[Type] = true;
        }
    }

    /// <summary>Cosmic Empowerment: Every 8s minions gain for 4s — +25% summon damage.</summary>
    public class CosmicEmpowerment : ModBuff
    {
        public override string Texture => "MagnumOpus/Assets/BuffIcons/CosmicEmpowerment";
        public override void SetStaticDefaults()
        {
            Main.buffNoTimeDisplay[Type] = false;
            Main.debuff[Type] = false;
        }
        public override void Update(Player player, ref int buffIndex)
        {
            player.GetDamage(DamageClass.Summon) += 0.25f;
        }
    }
}
