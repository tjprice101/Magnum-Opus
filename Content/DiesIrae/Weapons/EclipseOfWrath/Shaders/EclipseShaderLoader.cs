using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria.Graphics.Shaders;
using Terraria.ModLoader;

namespace MagnumOpus.Content.DiesIrae.Weapons.EclipseOfWrath.Shaders
{
    /// <summary>
    /// Loads Eclipse of Wrath dedicated shaders.
    /// EclipseOrb: Dark sun corona effect with pulse.
    /// WrathShardTrail: Tracking shard trail with solar gradient.
    /// </summary>
    [Autoload(Side = ModSide.Client)]
    public sealed class EclipseShaderLoader : ModSystem
    {
        internal static Asset<Effect> EclipseOrbShader;

        public const string EclipseOrbKey = "MagnumOpus:EclipseOrb";
        public const string WrathShardTrailKey = "MagnumOpus:WrathShardTrail";

        public static bool HasEclipseOrb => EclipseOrbShader?.IsLoaded == true;

        public override void PostSetupContent()
        {
            try
            {
                EclipseOrbShader = Mod.Assets.Request<Effect>("Effects/DiesIrae/EclipseOfWrath/EclipseOrb", AssetRequestMode.ImmediateLoad);
                if (EclipseOrbShader?.Value != null)
                {
                    GameShaders.Misc[EclipseOrbKey] = new MiscShaderData(EclipseOrbShader, "EclipseOrbMain");
                    GameShaders.Misc[WrathShardTrailKey] = new MiscShaderData(EclipseOrbShader, "WrathShardTrailMain");
                }
            }
            catch { EclipseOrbShader = null; }
        }
    }
}
