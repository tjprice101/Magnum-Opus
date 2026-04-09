using Terraria;
using Terraria.ModLoader;

namespace MagnumOpus.Content.SwanLake.Accessories
{
    /// <summary>Glorious Swan: Magic attacks 10% chance — next 5 magic casts consume no mana (White Swan Mode).</summary>
    public class GloriousSwan : ModBuff
    {
        public override string Texture => "MagnumOpus/Assets/BuffIcons/GloriousSwan";
        public override void SetStaticDefaults()
        {
            Main.buffNoTimeDisplay[Type] = false;
            Main.debuff[Type] = false;
        }
    }

    /// <summary>Swan of the Black Flame: Magic attacks 10% chance — next 5 magic attacks deal double damage (Black Swan Mode).</summary>
    public class SwanOfTheBlackFlame : ModBuff
    {
        public override string Texture => "MagnumOpus/Assets/BuffIcons/SwanOfTheBlackFlame";
        public override void SetStaticDefaults()
        {
            Main.buffNoTimeDisplay[Type] = false;
            Main.debuff[Type] = false;
        }
    }

    /// <summary>Odette's Wonder: Melee attacks 5% chance — +5% damage output for 5s (White Swan Mode).</summary>
    public class OdettesWonder : ModBuff
    {
        public override string Texture => "MagnumOpus/Assets/BuffIcons/OdettesWonder";
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

    /// <summary>Odile's Grace: Melee attacks +5% chance for +25% melee speed for 3s (Black Swan Mode).</summary>
    public class OdilesGrace : ModBuff
    {
        public override string Texture => "MagnumOpus/Assets/BuffIcons/OdilesGrace";
        public override void SetStaticDefaults()
        {
            Main.buffNoTimeDisplay[Type] = false;
            Main.debuff[Type] = false;
        }
        public override void Update(Player player, ref int buffIndex)
        {
            player.GetAttackSpeed(DamageClass.Melee) += 0.25f;
        }
    }

    /// <summary>Swan's Aria: Shots 5% chance to inflict — 5x increased damage (White Swan debuff on enemy).</summary>
    public class SwansAria : ModBuff
    {
        public override string Texture => "MagnumOpus/Assets/BuffIcons/SwansAria";
        public override void SetStaticDefaults()
        {
            Main.debuff[Type] = true;
            Main.pvpBuff[Type] = true;
            Main.buffNoSave[Type] = true;
        }
    }

    /// <summary>Swan's Opera: Shots 10% chance to inflict for 3s — 1.5x damage + player gains +20% fire rate (Black Swan debuff).</summary>
    public class SwansOpera : ModBuff
    {
        public override string Texture => "MagnumOpus/Assets/BuffIcons/SwansOpera";
        public override void SetStaticDefaults()
        {
            Main.debuff[Type] = true;
            Main.pvpBuff[Type] = true;
            Main.buffNoSave[Type] = true;
        }
    }

    /// <summary>Dying Swan's Grace: While airborne with Swan's Chromatic Diadem — applies Odile's Beauty on enemies for 5s.</summary>
    public class DyingSwansGrace : ModBuff
    {
        public override string Texture => "MagnumOpus/Assets/BuffIcons/DyingSwansGrace";
        public override void SetStaticDefaults()
        {
            Main.buffNoTimeDisplay[Type] = false;
            Main.debuff[Type] = false;
        }
    }
}
