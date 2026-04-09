using Terraria;
using Terraria.ModLoader;

namespace MagnumOpus.Content.OdeToJoy.Accessories
{
    /// <summary>Jubilant Tempo: +20% attack speed, +15% melee damage. Lifesteal handled in ModPlayer.</summary>
    public class JubilantTempoBuff : ModBuff
    {
        public override string Texture => "MagnumOpus/Assets/BuffIcons/JubilantTempoBuff";
        public override void SetStaticDefaults() { Main.buffNoTimeDisplay[Type] = false; Main.debuff[Type] = false; }
        public override void Update(Player player, ref int buffIndex)
        {
            player.GetAttackSpeed(DamageClass.Melee) += 0.20f;
            player.GetDamage(DamageClass.Melee) += 0.15f;
        }
    }

    /// <summary>Joyous Bloom: +15% magic damage. Mana restore handled in ModPlayer.</summary>
    public class JoyousBloomBuff : ModBuff
    {
        public override string Texture => "MagnumOpus/Assets/BuffIcons/JoyousBloomBuff";
        public override void SetStaticDefaults() { Main.buffNoTimeDisplay[Type] = false; Main.debuff[Type] = false; }
        public override void Update(Player player, ref int buffIndex)
        {
            player.GetDamage(DamageClass.Magic) += 0.15f;
        }
    }

    /// <summary>Triumphant Volley: +20% ranged damage, +10% ranged attack speed.</summary>
    public class TriumphantVolleyBuff : ModBuff
    {
        public override string Texture => "MagnumOpus/Assets/BuffIcons/TriumphantVolleyBuff";
        public override void SetStaticDefaults() { Main.buffNoTimeDisplay[Type] = false; Main.debuff[Type] = false; }
        public override void Update(Player player, ref int buffIndex)
        {
            player.GetDamage(DamageClass.Ranged) += 0.20f;
            player.GetAttackSpeed(DamageClass.Ranged) += 0.10f;
        }
    }

    /// <summary>Ovation: +10% all damage, +5% crit per stack (max 3 stacks from ranged kills).</summary>
    public class OvationBuff : ModBuff
    {
        public override string Texture => "MagnumOpus/Assets/BuffIcons/OvationBuff";
        public override void SetStaticDefaults() { Main.buffNoTimeDisplay[Type] = false; Main.debuff[Type] = false; }
        public override void Update(Player player, ref int buffIndex)
        {
            int stacks = player.GetModPlayer<OdeToJoyAccessoryPlayer>().ovationStacks;
            player.GetDamage(DamageClass.Generic) += 0.10f * stacks;
            player.GetCritChance(DamageClass.Generic) += 5 * stacks;
        }
    }

    /// <summary>Refrain: +25% minion damage, doubled Hymnal Anchor proc chance. Timed every 15s.</summary>
    public class RefrainBuff : ModBuff
    {
        public override string Texture => "MagnumOpus/Assets/BuffIcons/RefrainBuff";
        public override void SetStaticDefaults() { Main.buffNoTimeDisplay[Type] = false; Main.debuff[Type] = false; }
        public override void Update(Player player, ref int buffIndex)
        {
            player.GetDamage(DamageClass.Summon) += 0.25f;
        }
    }

    /// <summary>Ode to Joy Wing HP Amplification: 50% damage reduction (doubles effective HP) for 50 seconds.</summary>
    public class OdeToJoyWingAmplifyBuff : ModBuff
    {
        public override string Texture => "MagnumOpus/Assets/BuffIcons/OdeToJoyWingAmplifyBuff";
        public override void SetStaticDefaults() { Main.buffNoTimeDisplay[Type] = false; Main.debuff[Type] = false; }
        public override void Update(Player player, ref int buffIndex)
        {
            player.endurance += 0.50f; // 50% damage reduction = doubles effective HP
        }
    }
}