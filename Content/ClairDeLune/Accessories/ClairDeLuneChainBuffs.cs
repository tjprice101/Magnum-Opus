using Terraria;
using Terraria.ModLoader;

namespace MagnumOpus.Content.ClairDeLune.Accessories
{
    // === PLAYER BUFFS ===

    /// <summary>Nocturne: +3 life regen, +5% melee damage for 3s on melee kill.</summary>
    public class Nocturne : ModBuff
    {
        public override string Texture => "MagnumOpus/Assets/BuffIcons/Nocturne";
        public override void SetStaticDefaults()
        {
            Main.buffNoTimeDisplay[Type] = false;
            Main.debuff[Type] = false;
        }
        public override void Update(Player player, ref int buffIndex)
        {
            player.lifeRegen += 3;
            player.GetDamage(DamageClass.Melee) += 0.05f;
        }
    }

    /// <summary>Reverie's Flow: Restores 5 mana over 2s (5% magic hit proc).</summary>
    public class ReveriesFlow : ModBuff
    {
        public override string Texture => "MagnumOpus/Assets/BuffIcons/ReveriesFlow";
        public override void SetStaticDefaults()
        {
            Main.buffNoTimeDisplay[Type] = false;
            Main.debuff[Type] = false;
        }
        public override void Update(Player player, ref int buffIndex)
        {
            player.manaRegenBonus += 15;
        }
    }

    /// <summary>Moonbeam Focus: +5% ranged damage, +3% ranged crit for 2s on ranged crit at night.</summary>
    public class MoonbeamFocus : ModBuff
    {
        public override string Texture => "MagnumOpus/Assets/BuffIcons/MoonbeamFocus";
        public override void SetStaticDefaults()
        {
            Main.buffNoTimeDisplay[Type] = false;
            Main.debuff[Type] = false;
        }
        public override void Update(Player player, ref int buffIndex)
        {
            player.GetDamage(DamageClass.Ranged) += 0.05f;
            player.GetCritChance(DamageClass.Ranged) += 3;
        }
    }

    /// <summary>Lightspeed Reverie: Brief invincibility frames + contact damage at 250 momentum.</summary>
    public class LightspeedReverie : ModBuff
    {
        public override string Texture => "MagnumOpus/Assets/BuffIcons/LightspeedReverie";
        public override void SetStaticDefaults()
        {
            Main.buffNoTimeDisplay[Type] = false;
            Main.debuff[Type] = false;
        }
        public override void Update(Player player, ref int buffIndex)
        {
            player.noKnockback = true;
        }
    }

    // === ENEMY DEBUFFS ===

    /// <summary>Berceuse: -5 def, -10% speed.</summary>
    public class Berceuse : ModBuff
    {
        public override string Texture => "MagnumOpus/Assets/BuffIcons/Berceuse";
        public override void SetStaticDefaults()
        {
            Main.debuff[Type] = true;
            Main.pvpBuff[Type] = true;
            Main.buffNoSave[Type] = true;
        }
    }

    /// <summary>Pas sur la Neige: -15% speed.</summary>
    public class PasSurLaNeige : ModBuff
    {
        public override string Texture => "MagnumOpus/Assets/BuffIcons/PasSurLaNeige";
        public override void SetStaticDefaults()
        {
            Main.debuff[Type] = true;
            Main.pvpBuff[Type] = true;
            Main.buffNoSave[Type] = true;
        }
    }

    /// <summary>Voiles: 15% miss chance.</summary>
    public class Voiles : ModBuff
    {
        public override string Texture => "MagnumOpus/Assets/BuffIcons/Voiles";
        public override void SetStaticDefaults()
        {
            Main.debuff[Type] = true;
            Main.pvpBuff[Type] = true;
            Main.buffNoSave[Type] = true;
        }
    }

    /// <summary>Brumes: -12% speed.</summary>
    public class Brumes : ModBuff
    {
        public override string Texture => "MagnumOpus/Assets/BuffIcons/Brumes";
        public override void SetStaticDefaults()
        {
            Main.debuff[Type] = true;
            Main.pvpBuff[Type] = true;
            Main.buffNoSave[Type] = true;
        }
    }
}
