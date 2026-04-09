using Terraria;
using Terraria.ModLoader;

namespace MagnumOpus.Content.DiesIrae.Accessories
{
    /// <summary>Day of Wrath: +30% melee damage, melee attacks inflict Cursed Inferno, execute threshold 25%. Kills extend duration.</summary>
    public class DayOfWrathBuff : ModBuff
    {
        public override string Texture => "MagnumOpus/Assets/BuffIcons/DayOfWrathBuff";
        public override void SetStaticDefaults() { Main.buffNoTimeDisplay[Type] = false; Main.debuff[Type] = false; }
        public override void Update(Player player, ref int buffIndex)
        {
            player.GetDamage(DamageClass.Melee) += 0.30f;
        }
    }

    /// <summary>Dies Irae Wing HP Amplification: 50% damage reduction (doubles effective HP) for 45 seconds.</summary>
    public class DiesIraeWingAmplifyBuff : ModBuff
    {
        public override string Texture => "MagnumOpus/Assets/BuffIcons/DiesIraeWingAmplifyBuff";
        public override void SetStaticDefaults() { Main.buffNoTimeDisplay[Type] = false; Main.debuff[Type] = false; }
        public override void Update(Player player, ref int buffIndex)
        {
            player.endurance += 0.50f; // 50% damage reduction = doubles effective HP
        }
    }
}