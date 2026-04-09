using Terraria;
using Terraria.ModLoader;

namespace MagnumOpus.Common
{
    /// <summary>Wing HP Amplification buff for Moonlight Sonata wings: 50% DR for 10 seconds.</summary>
    public class MoonlightWingAmplifyBuff : ModBuff
    {
        public override string Texture => "MagnumOpus/Assets/BuffIcons/MoonlightWingAmplifyBuff";
        public override void SetStaticDefaults() { Main.buffNoTimeDisplay[Type] = false; Main.debuff[Type] = false; }
        public override void Update(Player player, ref int buffIndex) { player.endurance += 0.50f; }
    }

    /// <summary>Wing HP Amplification buff for Eroica wings: 50% DR for 13 seconds.</summary>
    public class EroicaWingAmplifyBuff : ModBuff
    {
        public override string Texture => "MagnumOpus/Assets/BuffIcons/EroicaWingAmplifyBuff";
        public override void SetStaticDefaults() { Main.buffNoTimeDisplay[Type] = false; Main.debuff[Type] = false; }
        public override void Update(Player player, ref int buffIndex) { player.endurance += 0.50f; }
    }

    /// <summary>Wing HP Amplification buff for La Campanella wings: 50% DR for 15 seconds.</summary>
    public class LaCampanellaWingAmplifyBuff : ModBuff
    {
        public override string Texture => "MagnumOpus/Assets/BuffIcons/LaCampanellaWingAmplifyBuff";
        public override void SetStaticDefaults() { Main.buffNoTimeDisplay[Type] = false; Main.debuff[Type] = false; }
        public override void Update(Player player, ref int buffIndex) { player.endurance += 0.50f; }
    }

    /// <summary>Wing HP Amplification buff for Enigma Variations wings: 50% DR for 20 seconds.</summary>
    public class EnigmaWingAmplifyBuff : ModBuff
    {
        public override string Texture => "MagnumOpus/Assets/BuffIcons/EnigmaWingAmplifyBuff";
        public override void SetStaticDefaults() { Main.buffNoTimeDisplay[Type] = false; Main.debuff[Type] = false; }
        public override void Update(Player player, ref int buffIndex) { player.endurance += 0.50f; }
    }

    /// <summary>Wing HP Amplification buff for Swan Lake wings: 50% DR for 25 seconds.</summary>
    public class SwanLakeWingAmplifyBuff : ModBuff
    {
        public override string Texture => "MagnumOpus/Assets/BuffIcons/SwanLakeWingAmplifyBuff";
        public override void SetStaticDefaults() { Main.buffNoTimeDisplay[Type] = false; Main.debuff[Type] = false; }
        public override void Update(Player player, ref int buffIndex) { player.endurance += 0.50f; }
    }

    /// <summary>Wing HP Amplification buff for Fate wings: 50% DR for 35 seconds.</summary>
    public class FateWingAmplifyBuff : ModBuff
    {
        public override string Texture => "MagnumOpus/Assets/BuffIcons/FateWingAmplifyBuff";
        public override void SetStaticDefaults() { Main.buffNoTimeDisplay[Type] = false; Main.debuff[Type] = false; }
        public override void Update(Player player, ref int buffIndex) { player.endurance += 0.50f; }
    }
}
