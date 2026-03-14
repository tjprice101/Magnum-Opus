using Terraria;
using Terraria.ModLoader;

namespace MagnumOpus.Common.Systems.Bosses
{
    /// <summary>
    /// Global boss index tracker (Calamity GlobalNPC pattern).
    /// Tracks active boss NPC.whoAmI indices so sky, screen shader, music,
    /// and VFX systems can cheaply query boss state from anywhere.
    /// Updated every tick; cleared on boss despawn/death.
    /// </summary>
    public class BossIndexTracker : ModSystem
    {
        // Major bosses
        public static int EroicaRetribution = -1;
        public static int LaCampanellaChime = -1;
        public static int SwanLakeFractal = -1;
        public static int EnigmaHollowMystery = -1;
        public static int FateWardenOfMelodies = -1;
        public static int NachtmusikQueen = -1;
        public static int OdeToJoyConductor = -1;
        public static int DiesIraeHerald = -1;

        // Seasonal bosses
        public static int Autunno = -1;
        public static int Primavera = -1;
        public static int LEstate = -1;
        public static int LInverno = -1;

        // Boss minions
        public static int FlamesOfValor1 = -1;
        public static int FlamesOfValor2 = -1;
        public static int FlamesOfValor3 = -1;
        public static int MovementI = -1;
        public static int MovementII = -1;
        public static int MovementIII = -1;

        // Boss phase tracking (set by boss AI)
        public static int EroicaPhase = 0;
        public static bool EroicaEnraged = false;
        public static int LaCampanellaPhase = 0;
        public static int SwanLakeMood = 0; // 0=Graceful, 1=Tempest, 2=DyingSwan
        public static int EnigmaPhase = 0;
        public static bool EnigmaEnraged = false;
        public static int FatePhase = 0;
        public static bool FateAwakened = false;
        public static int NachtmusikPhase = 0;
        public static int OdeToJoyPhase = 0;
        public static int DiesIraePhase = 0;

        /// <summary>True if ANY tracked boss is currently active.</summary>
        public static bool AnyBossActive =>
            EroicaRetribution != -1 || LaCampanellaChime != -1 || SwanLakeFractal != -1 ||
            EnigmaHollowMystery != -1 || FateWardenOfMelodies != -1 || NachtmusikQueen != -1 ||
            OdeToJoyConductor != -1 || DiesIraeHerald != -1 ||
            Autunno != -1 || Primavera != -1 || LEstate != -1 || LInverno != -1;

        /// <summary>Returns the active boss NPC, or null.</summary>
        public static NPC GetActiveBoss(int index) =>
            index >= 0 && index < Main.maxNPCs && Main.npc[index].active ? Main.npc[index] : null;

        public override void PreUpdateNPCs()
        {
            // Validate indices — if NPC despawned or died, clear the index
            ValidateIndex(ref EroicaRetribution);
            ValidateIndex(ref LaCampanellaChime);
            ValidateIndex(ref SwanLakeFractal);
            ValidateIndex(ref EnigmaHollowMystery);
            ValidateIndex(ref FateWardenOfMelodies);
            ValidateIndex(ref NachtmusikQueen);
            ValidateIndex(ref OdeToJoyConductor);
            ValidateIndex(ref DiesIraeHerald);
            ValidateIndex(ref Autunno);
            ValidateIndex(ref Primavera);
            ValidateIndex(ref LEstate);
            ValidateIndex(ref LInverno);
            ValidateIndex(ref FlamesOfValor1);
            ValidateIndex(ref FlamesOfValor2);
            ValidateIndex(ref FlamesOfValor3);
            ValidateIndex(ref MovementI);
            ValidateIndex(ref MovementII);
            ValidateIndex(ref MovementIII);
        }

        private static void ValidateIndex(ref int index)
        {
            if (index < 0) return;
            if (index >= Main.maxNPCs || !Main.npc[index].active)
                index = -1;
        }

        public override void OnWorldUnload()
        {
            ResetAll();
        }

        public static void ResetAll()
        {
            EroicaRetribution = -1;
            LaCampanellaChime = -1;
            SwanLakeFractal = -1;
            EnigmaHollowMystery = -1;
            FateWardenOfMelodies = -1;
            NachtmusikQueen = -1;
            OdeToJoyConductor = -1;
            DiesIraeHerald = -1;
            Autunno = -1;
            Primavera = -1;
            LEstate = -1;
            LInverno = -1;
            FlamesOfValor1 = -1;
            FlamesOfValor2 = -1;
            FlamesOfValor3 = -1;
            MovementI = -1;
            MovementII = -1;
            MovementIII = -1;

            EroicaPhase = 0;
            EroicaEnraged = false;
            LaCampanellaPhase = 0;
            SwanLakeMood = 0;
            EnigmaPhase = 0;
            EnigmaEnraged = false;
            EnigmaHollowMystery = -1;
            FatePhase = 0;
            FateAwakened = false;
            NachtmusikPhase = 0;
            OdeToJoyPhase = 0;
            DiesIraePhase = 0;
        }
    }
}
