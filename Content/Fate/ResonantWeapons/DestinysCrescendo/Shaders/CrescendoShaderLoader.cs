using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.ModLoader;

namespace MagnumOpus.Content.Fate.ResonantWeapons.DestinysCrescendo
{
    /// <summary>
    /// Loads and caches all Destiny's Crescendo HLSL shaders.
    /// 
    /// Keys (5 from 4 .fx files):
    ///   CrescendoBeamTrail   — cosmic beam trail (technique "BeamMain")
    ///   CrescendoBeamGlow    — same .fx, technique "BeamGlow"
    ///   CrescendoAuraGlow    — deity ambient aura, radial pulse
    ///   CrescendoSlashArc    — slash impact arc shader
    ///   CrescendoSummonBloom — summoning explosion bloom
    /// </summary>
    public class CrescendoShaderLoader : ModSystem
    {
        public static Effect CrescendoBeamTrail   { get; private set; }
        public static Effect CrescendoBeamGlow    { get; private set; }
        public static Effect CrescendoAuraGlow    { get; private set; }
        public static Effect CrescendoSlashArc    { get; private set; }
        public static Effect CrescendoSummonBloom { get; private set; }

        public override void Load()
        {
            if (Main.dedServ) return;

            CrescendoBeamTrail   = SafeLoad("Effects/Fate/DestinysCrescendo/CrescendoBeamTrail");
            CrescendoAuraGlow    = SafeLoad("Effects/Fate/DestinysCrescendo/CrescendoAuraGlow");
            CrescendoSlashArc    = SafeLoad("Effects/Fate/DestinysCrescendo/CrescendoSlashArc");
            CrescendoSummonBloom = SafeLoad("Effects/Fate/DestinysCrescendo/CrescendoSummonBloom");

            // BeamGlow is the same Effect as BeamTrail — caller switches technique
            CrescendoBeamGlow = CrescendoBeamTrail;
        }

        public override void Unload()
        {
            CrescendoBeamTrail   = null;
            CrescendoBeamGlow    = null;
            CrescendoAuraGlow    = null;
            CrescendoSlashArc    = null;
            CrescendoSummonBloom = null;
        }

        private Effect SafeLoad(string path)
        {
            try
            {
                return Mod.Assets.Request<Effect>(path, AssetRequestMode.ImmediateLoad).Value;
            }
            catch
            {
                // Shader compilation may fail on some platforms; renderer has a fallback
                return null;
            }
        }
    }
}
