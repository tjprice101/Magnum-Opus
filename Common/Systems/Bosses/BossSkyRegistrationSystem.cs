using Terraria;
using Terraria.Graphics.Effects;
using Terraria.ModLoader;
using MagnumOpus.Content.SwanLake.Bosses.Systems;
using MagnumOpus.Content.DiesIrae.Bosses.Systems;
using MagnumOpus.Content.Autumn.Bosses.Systems;
using MagnumOpus.Content.Spring.Bosses.Systems;
using MagnumOpus.Content.Summer.Bosses.Systems;
using MagnumOpus.Content.Winter.Bosses.Systems;

namespace MagnumOpus.Common.Systems.Bosses
{
    /// <summary>
    /// Registers all boss CustomSky implementations that don't already have
    /// their own ModSystem loader (the VFX/Screen sky effects for Eroica,
    /// La Campanella, Enigma, Fate, and Nachtmusik are self-registering).
    /// 
    /// Activation is handled by each boss's AI code:
    ///   SkyManager.Instance.Activate("MagnumOpus:SwanLakeSky");
    ///   SkyManager.Instance.Deactivate("MagnumOpus:SwanLakeSky");
    /// </summary>
    public class BossSkyRegistrationSystem : ModSystem
    {
        // Sky registration keys — use these from boss AI code
        public const string SwanLakeKey = "MagnumOpus:SwanLakeSky";
        public const string DiesIraeKey = "MagnumOpus:DiesIraeSky";
        public const string AutunnoKey = "MagnumOpus:AutunnoSky";
        public const string PrimaveraKey = "MagnumOpus:PrimaveraSky";
        public const string LEstateKey = "MagnumOpus:LEstateSky";
        public const string LInvernoKey = "MagnumOpus:LInvernoSky";

        public override void Load()
        {
            if (Main.dedServ)
                return;

            SkyManager.Instance[SwanLakeKey] = new SwanLakeSky();
            SkyManager.Instance[DiesIraeKey] = new DiesIraeSky();
            SkyManager.Instance[AutunnoKey] = new AutunnoSky();
            SkyManager.Instance[PrimaveraKey] = new PrimaveraSky();
            SkyManager.Instance[LEstateKey] = new LEstateSky();
            SkyManager.Instance[LInvernoKey] = new LInvernoSky();
        }
    }
}
