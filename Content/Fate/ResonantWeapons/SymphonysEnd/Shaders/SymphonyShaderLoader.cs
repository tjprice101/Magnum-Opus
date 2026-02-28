using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.ModLoader;

namespace MagnumOpus.Content.Fate.ResonantWeapons.SymphonysEnd
{
    /// <summary>
    /// Loads and caches all Symphony's End HLSL shaders.
    /// 
    /// Keys (5):
    ///   SymphonySpiralTrail  — main blade trail (technique "SpiralMain")
    ///   SymphonySpiralGlow   — same .fx, technique "SpiralGlow"
    ///   SymphonyFragmentTrail — fragment scatter trail
    ///   SymphonyShatterBloom  — shatter impact bloom
    ///   SymphonyCrackle       — wand-tip crackle aura
    /// </summary>
    public class SymphonyShaderLoader : ModSystem
    {
        public static Effect SymphonySpiralTrail  { get; private set; }
        public static Effect SymphonySpiralGlow   { get; private set; }
        public static Effect SymphonyFragmentTrail { get; private set; }
        public static Effect SymphonyShatterBloom  { get; private set; }
        public static Effect SymphonyCrackle       { get; private set; }

        public override void Load()
        {
            if (Main.dedServ) return;

            SymphonySpiralTrail  = SafeLoad("Effects/Fate/SymphonysEnd/SymphonySpiralTrail");
            SymphonyFragmentTrail = SafeLoad("Effects/Fate/SymphonysEnd/SymphonyFragmentTrail");
            SymphonyShatterBloom  = SafeLoad("Effects/Fate/SymphonysEnd/SymphonyShatterBloom");
            SymphonyCrackle       = SafeLoad("Effects/Fate/SymphonysEnd/SymphonyCrackle");

            // Glow is the same Effect as SpiralTrail — caller switches technique
            SymphonySpiralGlow = SymphonySpiralTrail;
        }

        public override void Unload()
        {
            SymphonySpiralTrail = null;
            SymphonySpiralGlow  = null;
            SymphonyFragmentTrail = null;
            SymphonyShatterBloom  = null;
            SymphonyCrackle       = null;
        }

        private Effect SafeLoad(string path)
        {
            try
            {
                return Mod.Assets.Request<Effect>(path, AssetRequestMode.ImmediateLoad).Value;
            }
            catch
            {
                // Shader compilation may fail on some platforms; trail renderer has a fallback
                return null;
            }
        }
    }
}
